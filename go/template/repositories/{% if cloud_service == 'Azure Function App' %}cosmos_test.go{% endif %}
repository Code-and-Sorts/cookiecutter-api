package repositories

import (
	"errors"
	"fmt"
	"net/http"
	"testing"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func cosmosError(statusCode int, substatus string) error {
	header := http.Header{}
	if substatus != "" {
		header.Set("x-ms-substatus", substatus)
	}
	return fmt.Errorf("read item: %w", &azcore.ResponseError{
		StatusCode:  statusCode,
		RawResponse: &http.Response{StatusCode: statusCode, Header: header},
	})
}

func TestIsItemNotFound_MissingItem(t *testing.T) {
	assert.True(t, IsItemNotFound(cosmosError(http.StatusNotFound, "")))
	assert.True(t, IsItemNotFound(cosmosError(http.StatusNotFound, "0")))
	assert.True(t, IsItemNotFound(&azcore.ResponseError{StatusCode: http.StatusNotFound}))
}

func TestIsItemNotFound_MissingContainerIsNotItemNotFound(t *testing.T) {
	assert.False(t, IsItemNotFound(cosmosError(http.StatusNotFound, "1003")))
}

func TestCosmosClientOptions_BoundsRetries(t *testing.T) {
	options := CosmosClientOptions("https://account.documents.azure.com:443/", false)

	assert.Equal(t, int32(1), options.Retry.MaxRetries)
	assert.Equal(t, 3*time.Second, options.Retry.TryTimeout)
}

func TestCosmosClientOptions_KeepsTLSVerificationWithoutTheEmulatorFlag(t *testing.T) {
	assert.Nil(t, CosmosClientOptions("https://localhost:8081/", false).Transport)
}

func TestCosmosClientOptions_PlainHTTPEmulatorNeedsNoTLSChanges(t *testing.T) {
	assert.Nil(t, CosmosClientOptions("http://localhost:8081/", true).Transport)
}

func TestCosmosClientOptions_HTTPSEmulatorSkipsCertificateVerification(t *testing.T) {
	options := CosmosClientOptions("https://localhost:8081/", true)

	client, ok := options.Transport.(*http.Client)
	require.True(t, ok)
	transport, ok := client.Transport.(*http.Transport)
	require.True(t, ok)
	assert.True(t, transport.TLSClientConfig.InsecureSkipVerify)
	assert.Equal(t, int32(1), options.Retry.MaxRetries)
}

func TestIsItemNotFound_OtherErrors(t *testing.T) {
	assert.False(t, IsItemNotFound(cosmosError(http.StatusServiceUnavailable, "")))
	assert.False(t, IsItemNotFound(errors.New("connection refused")))
}
