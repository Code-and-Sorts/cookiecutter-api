package repositories

import (
	"encoding/base64"
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

const cosmosEndpoint = "https://account.documents.azure.com:443/"

func TestNewCosmosClient_UsesTheKeyWhenSet(t *testing.T) {
	t.Setenv("AZURE_TOKEN_CREDENTIALS", "invalid")

	client, err := NewCosmosClient(cosmosEndpoint, base64.StdEncoding.EncodeToString([]byte("key")), CosmosClientOptions(cosmosEndpoint, false))

	require.NoError(t, err)
	assert.Equal(t, cosmosEndpoint, client.Endpoint())
}

func TestNewCosmosClient_RejectsAMalformedKey(t *testing.T) {
	_, err := NewCosmosClient(cosmosEndpoint, "not a base64 key!", CosmosClientOptions(cosmosEndpoint, false))

	assert.Error(t, err)
}

func TestNewCosmosClient_UsesDefaultAzureCredentialWithoutAKey(t *testing.T) {
	t.Setenv("AZURE_TOKEN_CREDENTIALS", "invalid")

	_, err := NewCosmosClient(cosmosEndpoint, "", CosmosClientOptions(cosmosEndpoint, false))

	assert.ErrorContains(t, err, "AZURE_TOKEN_CREDENTIALS")
}

func TestNewCosmosClient_UsesTheManagedIdentityWithoutAKey(t *testing.T) {
	t.Setenv("AZURE_TOKEN_CREDENTIALS", "ManagedIdentityCredential")
	t.Setenv("AZURE_CLIENT_ID", "00000000-0000-0000-0000-000000000001")

	client, err := NewCosmosClient(cosmosEndpoint, "", CosmosClientOptions(cosmosEndpoint, false))

	require.NoError(t, err)
	assert.Equal(t, cosmosEndpoint, client.Endpoint())
}

func TestIsItemNotFound_OtherErrors(t *testing.T) {
	assert.False(t, IsItemNotFound(cosmosError(http.StatusServiceUnavailable, "")))
	assert.False(t, IsItemNotFound(errors.New("connection refused")))
}
