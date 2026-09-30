package repositories

import (
	"errors"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
)

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
