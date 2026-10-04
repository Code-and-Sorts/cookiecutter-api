package repositories

import (
	"crypto/tls"
	"errors"
	"net/http"
	"strings"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/azcore/policy"
	"github.com/Azure/azure-sdk-for-go/sdk/azidentity"
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
)

func CosmosClientOptions(endpoint string, emulator bool) *azcosmos.ClientOptions {
	// The SDK's first account read ignores the request deadline and could wait a minute on an unreachable Cosmos DB.
	options := &azcosmos.ClientOptions{ClientOptions: azcore.ClientOptions{
		Retry: policy.RetryOptions{
			MaxRetries: 1,
			TryTimeout: 3 * time.Second,
			RetryDelay: 500 * time.Millisecond,
		},
	}}
	if emulator && strings.HasPrefix(strings.ToLower(endpoint), "https://") {
		// Only an emulator serving HTTPS gets here; its certificate is self-signed.
		transport := http.DefaultTransport.(*http.Transport).Clone()
		transport.TLSClientConfig = &tls.Config{InsecureSkipVerify: true}
		options.Transport = &http.Client{Transport: transport}
	}
	return options
}

func NewCosmosClient(endpoint, key string, options *azcosmos.ClientOptions) (*azcosmos.Client, error) {
	if key != "" {
		cred, err := azcosmos.NewKeyCredential(key)
		if err != nil {
			return nil, err
		}
		return azcosmos.NewClientWithKey(endpoint, cred, options)
	}
	cred, err := azidentity.NewDefaultAzureCredential(nil)
	if err != nil {
		return nil, err
	}
	return azcosmos.NewClient(endpoint, cred, options)
}

// Only substatus 0 is a missing item; others (e.g. 1003, no such container) are config errors and stay 500s.
func IsItemNotFound(err error) bool {
	var respErr *azcore.ResponseError
	if !errors.As(err, &respErr) || respErr.StatusCode != http.StatusNotFound {
		return false
	}
	if respErr.RawResponse == nil {
		return true
	}
	substatus := respErr.RawResponse.Header.Get("x-ms-substatus")
	return substatus == "" || substatus == "0"
}
