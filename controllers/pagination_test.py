from .pagination import DEFAULT_LIST_LIMIT, MAX_LIST_LIMIT, coerce_limit


def describe_coerce_limit():
    def test_parses_valid_limit():
        assert coerce_limit("5") == 5

    def test_defaults_when_missing_or_invalid():
        assert coerce_limit(None) == DEFAULT_LIST_LIMIT
        assert coerce_limit("abc") == DEFAULT_LIST_LIMIT
        assert coerce_limit("0") == DEFAULT_LIST_LIMIT

    def test_caps_at_max_limit():
        assert coerce_limit(str(MAX_LIST_LIMIT + 1)) == MAX_LIST_LIMIT
