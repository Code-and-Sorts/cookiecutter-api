package handlers

import (
	"context"
	"log/slog"
	"net/http"
	"strings"
	"time"
	"unicode/utf8"

	"kittenclaws/models"
	"kittenclaws/utils"
)

// Covers every database call and SDK retry, so a failing database answers 500 inside the platform timeout.
const RequestTimeout = 8 * time.Second

const (
	UserIDHeader         = "X-User-Id"
	MaxUserIDLength      = 256
	UserIDTooLongMessage = "X-User-Id must be at most 256 characters."
)

// Checked before the body is read, so an oversized header never reaches the controller or the database.
func withUserID(header http.Header, call func(userID string) (any, error)) func() (any, error) {
	return func() (any, error) {
		userID := strings.TrimSpace(header.Get(UserIDHeader))
		if utf8.RuneCountInString(userID) > MaxUserIDLength {
			return nil, &models.ValidationError{Message: UserIDTooLongMessage}
		}
		return call(userID)
	}
}

func NewRouter() *http.ServeMux {
	mux := http.NewServeMux()
	mux.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		utils.WriteError(w, http.StatusNotFound, utils.NotFoundMessage)
	})
	return mux
}

func serve(w http.ResponseWriter, r *http.Request, status int, call func() (any, error)) {
	result, err := call()
	if err != nil {
		utils.DetectError(r.Context(), w, err)
		return
	}
	utils.WriteJSON(w, status, result)
}

// Registered without a method, so ServeMux only runs it when no method-specific pattern matches.
func methodNotAllowed(w http.ResponseWriter, r *http.Request) {
	utils.WriteError(w, http.StatusMethodNotAllowed, utils.MethodNotAllowedMessage)
}

func Middleware(next http.Handler) http.Handler {
	return Recover(LogRequests(WithRequestTimeout(next)))
}

func LogRequests(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		slog.Info("Processing request", "method", r.Method, "path", r.URL.Path)
		next.ServeHTTP(w, r)
	})
}

func WithRequestTimeout(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		ctx, cancel := context.WithTimeout(r.Context(), RequestTimeout)
		defer cancel()
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}

// Without this, net/http drops the connection on a panic instead of answering 500.
func Recover(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		defer func() {
			if recovered := recover(); recovered != nil {
				if recovered == http.ErrAbortHandler {
					panic(recovered)
				}
				utils.LogUnexpected(recovered)
				utils.WriteError(w, http.StatusInternalServerError, utils.UnexpectedErrorMessage)
			}
		}()
		next.ServeHTTP(w, r)
	})
}
