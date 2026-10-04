package repositories

import (
	"context"
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"

	"kittenclaws/models"
	"kittenclaws/utils"
)

const storedID = "28535ae3-2f1b-4e81-ba13-0f46a0c74ea0"

type storedRecord struct {
	models.BaseEntity
	Name string `json:"name"`
}

type storedStore = store[storedRecord, *storedRecord]

type fakeCosmos struct {
	status  int
	ifMatch string
}

func (f *fakeCosmos) Do(req *http.Request) (*http.Response, error) {
	status, body := http.StatusOK, `{}`
	switch {
	case req.Method == http.MethodGet && strings.Contains(req.URL.Path, "/docs/"):
		body = `{"id":"` + storedID + `","name":"stored","isDeleted":false}`
	case req.Method == http.MethodGet:
		body = `{"writableLocations":[],"readableLocations":[]}`
	case req.Method == http.MethodPut:
		f.ifMatch = req.Header.Get("If-Match")
		status = f.status
	}
	header := http.Header{"Etag": {`"etag-1"`}, "Content-Type": {"application/json"}}
	return &http.Response{StatusCode: status, Header: header, Body: io.NopCloser(strings.NewReader(body)), Request: req}, nil
}

func newFakeStore(t *testing.T, cosmos *fakeCosmos) *storedStore {
	key, err := azcosmos.NewKeyCredential("a2V5")
	require.NoError(t, err)
	client, err := azcosmos.NewClientWithKey("https://account.documents.azure.com:443/", key,
		&azcosmos.ClientOptions{ClientOptions: azcore.ClientOptions{Transport: cosmos}})
	require.NoError(t, err)
	container, err := client.NewContainer("database", "items")
	require.NoError(t, err)
	return newStore[storedRecord]("Item", container)
}

func TestStore_WritesBackOnlyIfTheETagItReadStillMatches(t *testing.T) {
	previous := slog.Default()
	slog.SetDefault(slog.New(slog.DiscardHandler))
	t.Cleanup(func() { slog.SetDefault(previous) })

	writes := map[string]func(*storedStore) error{
		"update": func(s *storedStore) error {
			_, err := s.update(context.Background(), storedID, "", func(*storedRecord) {})
			return err
		},
		"replace": func(s *storedStore) error {
			_, err := s.replace(context.Background(), &storedRecord{BaseEntity: models.BaseEntity{Id: storedID}}, "")
			return err
		},
		"delete": func(s *storedStore) error {
			return s.softDelete(context.Background(), storedID, "")
		},
	}
	for name, write := range writes {
		t.Run(name, func(t *testing.T) {
			cosmos := &fakeCosmos{status: http.StatusOK}
			require.NoError(t, write(newFakeStore(t, cosmos)))
			assert.Equal(t, `"etag-1"`, cosmos.ifMatch)

			cosmos.status = http.StatusPreconditionFailed
			err := write(newFakeStore(t, cosmos))
			require.Error(t, err)
			w := httptest.NewRecorder()
			utils.DetectError(context.Background(), w, err)
			assert.Equal(t, http.StatusInternalServerError, w.Code)
		})
	}
}
