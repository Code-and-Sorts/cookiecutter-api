package utils

import (
	"bytes"
	"context"
	"log/slog"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestNewLogger_ReturnsLogger(t *testing.T) {
	assert.NotNil(t, NewLogger())
}

func TestSplitLogger_WritesInfoToStdoutAndErrorsToStderr(t *testing.T) {
	var out, errOut bytes.Buffer
	logger := newSplitLogger(&out, &errOut).With("app", "test").WithGroup("request")

	logger.Info("served", "status", 200)
	logger.Warn("slow")
	logger.Error("failed", "status", 500)

	assert.Contains(t, out.String(), "msg=served")
	assert.Contains(t, out.String(), "msg=slow")
	assert.NotContains(t, out.String(), "failed")
	assert.Contains(t, errOut.String(), "msg=failed")
	assert.Contains(t, errOut.String(), "app=test")
	assert.Contains(t, errOut.String(), "request.status=500")
	assert.NotContains(t, errOut.String(), "served")
}

func TestSplitLogger_SkipsDebugByDefault(t *testing.T) {
	var out, errOut bytes.Buffer
	logger := newSplitLogger(&out, &errOut)

	assert.False(t, logger.Enabled(context.Background(), slog.LevelDebug))
	logger.Debug("hidden")

	assert.Empty(t, out.String())
}
