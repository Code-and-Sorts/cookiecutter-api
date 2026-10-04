package controllers

const (
	DefaultListLimit = 100
	MaxListLimit     = 1000
)

func CoerceLimit(limit int) int {
	if limit < 1 {
		return DefaultListLimit
	}
	if limit > MaxListLimit {
		return MaxListLimit
	}
	return limit
}
