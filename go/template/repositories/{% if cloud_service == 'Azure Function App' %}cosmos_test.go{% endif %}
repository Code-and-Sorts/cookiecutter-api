package repositories

import (
	"errors"
	"fmt"
	"net/http"
	"testing"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/stretchr/testify/assert"
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

func TestIsItemNotFound_OtherErrors(t *testing.T) {
	assert.False(t, IsItemNotFound(cosmosError(http.StatusServiceUnavailable, "")))
	assert.False(t, IsItemNotFound(errors.New("connection refused")))
}
