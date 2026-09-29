package utils

import (
	"context"
	"io"
	"log/slog"
	"os"
)

// NewLogger returns a logger that writes records below error level to stdout
// and error records to stderr, so hosts that read stderr as failures (such as
// the Azure Functions host) only see real errors there.
func NewLogger() *slog.Logger {
	return newSplitLogger(os.Stdout, os.Stderr)
}

func newSplitLogger(out, errOut io.Writer) *slog.Logger {
	return slog.New(splitHandler{
		out: slog.NewTextHandler(out, nil),
		err: slog.NewTextHandler(errOut, nil),
	})
}

type splitHandler struct {
	out slog.Handler
	err slog.Handler
}

func (h splitHandler) Enabled(ctx context.Context, level slog.Level) bool {
	return h.out.Enabled(ctx, level)
}

func (h splitHandler) Handle(ctx context.Context, record slog.Record) error {
	if record.Level >= slog.LevelError {
		return h.err.Handle(ctx, record)
	}
	return h.out.Handle(ctx, record)
}

func (h splitHandler) WithAttrs(attrs []slog.Attr) slog.Handler {
	return splitHandler{out: h.out.WithAttrs(attrs), err: h.err.WithAttrs(attrs)}
}

func (h splitHandler) WithGroup(name string) slog.Handler {
	return splitHandler{out: h.out.WithGroup(name), err: h.err.WithGroup(name)}
}
