package utils

import (
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestGetenv_ReturnsValue_WhenSet(t *testing.T) {
	t.Setenv("UTILS_TEST_SETTING", "configured")

	assert.Equal(t, "configured", Getenv("UTILS_TEST_SETTING", "fallback"))
}

func TestGetenv_ReturnsFallback_WhenUnsetOrEmpty(t *testing.T) {
	t.Setenv("UTILS_TEST_SETTING", "")

	assert.Equal(t, "fallback", Getenv("UTILS_TEST_SETTING", "fallback"))
	assert.Equal(t, "fallback", Getenv("UTILS_TEST_SETTING_NEVER_SET", "fallback"))
}
