{%- set used_ops = resources | map(attribute='operations') | sum(start=[]) -%}
{%- set uses_body = ('create' in used_ops) or ('update' in used_ops) or ('replace' in used_ops) -%}
{%- set uses_models = uses_body or ('get_by_id' in used_ops) or ('list' in used_ops) -%}
package controllers

import (
{%- if uses_body %}
	"bytes"
{%- endif %}
	"context"
{%- if uses_body %}
	"encoding/json"
{%- endif %}
	"testing"
{%- if uses_models %}

	"{{project_endpoint}}/models"
{%- endif %}
	"{{project_endpoint}}/services"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

func TestCoerceLimit_ClampsToSupportedRange(t *testing.T) {
	assert.Equal(t, DefaultListLimit, CoerceLimit(0))
	assert.Equal(t, DefaultListLimit, CoerceLimit(-5))
	assert.Equal(t, 25, CoerceLimit(25))
	assert.Equal(t, MaxListLimit, CoerceLimit(999999))
}
{% for resource in resources %}
{%- set r = resource.name %}
type Mock{{ r }}Service struct {
	mock.Mock
}
{%- if "get_by_id" in resource.operations %}

func (m *Mock{{ r }}Service) Get(ctx context.Context, id string) (*models.{{ r }}Dto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{ r }}Dto), args.Error(1)
}
{%- endif %}
{%- if "list" in resource.operations %}

func (m *Mock{{ r }}Service) GetList(ctx context.Context, limit int) ([]models.{{ r }}Dto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.{{ r }}Dto), args.Error(1)
}
{%- endif %}
{%- if "create" in resource.operations %}

func (m *Mock{{ r }}Service) Create(ctx context.Context, req models.Create{{ r }}Request) (*models.{{ r }}Dto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{ r }}Dto), args.Error(1)
}
{%- endif %}
{%- if "update" in resource.operations %}

func (m *Mock{{ r }}Service) Update(ctx context.Context, req models.Update{{ r }}Request) (*models.{{ r }}Dto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{ r }}Dto), args.Error(1)
}
{%- endif %}
{%- if "replace" in resource.operations %}

func (m *Mock{{ r }}Service) Replace(ctx context.Context, req models.Replace{{ r }}Request) (*models.{{ r }}Dto, error) {
	args := m.Called(ctx, req)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{ r }}Dto), args.Error(1)
}
{%- endif %}
{%- if "delete" in resource.operations %}

func (m *Mock{{ r }}Service) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}
{%- endif %}

func newTest{{ r }}Controller(mockService *Mock{{ r }}Service) {{ r }}Controller {
	validator, _ := services.NewSchemaValidator(map[string]string{
		"create_request":  CreateRequestSchema,
		"update_request":  UpdateRequestSchema,
		"replace_request": ReplaceRequestSchema,
	})
	return New{{ r }}Controller(mockService, validator)
}
{%- if "get_by_id" in resource.operations %}

func TestGet{{ r }}_Returns{{ r }}Dto(t *testing.T) {
	mockService := new(Mock{{ r }}Service)
	controller := newTest{{ r }}Controller(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.{{ r }}Dto{Id: id, Name: "mock{{ r }}"}
	mockService.On("Get", mock.Anything, id).Return(expected, nil)

	result, err := controller.Get(context.Background(), id)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Get", mock.Anything, id)
}
{%- endif %}
{%- if "list" in resource.operations %}

func TestGet{{ r }}List_ReturnsListOf{{ r }}Dto(t *testing.T) {
	mockService := new(Mock{{ r }}Service)
	controller := newTest{{ r }}Controller(mockService)
	expected := []models.{{ r }}Dto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mock{{ r }}1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mock{{ r }}2"},
	}
	mockService.On("GetList", mock.Anything, DefaultListLimit).Return(expected, nil)

	result, err := controller.GetList(context.Background(), 0)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "GetList", mock.Anything, DefaultListLimit)
}
{%- endif %}
{%- if "create" in resource.operations %}

func TestCreate{{ r }}_ReturnsCreated{{ r }}Dto(t *testing.T) {
	mockService := new(Mock{{ r }}Service)
	controller := newTest{{ r }}Controller(mockService)
	createReq := models.Create{{ r }}Request{Name: "mockCreate{{ r }}", CreatedBy: "TestUser", UpdatedBy: "TestUser"}
	expected := &models.{{ r }}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreate{{ r }}"}
	mockService.On("Create", mock.Anything, createReq).Return(expected, nil)

	body, _ := json.Marshal(createReq)

	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Create", mock.Anything, createReq)
}

func TestCreate{{ r }}_ReturnsValidationError_WhenNameIsEmpty(t *testing.T) {
	mockService := new(Mock{{ r }}Service)
	controller := newTest{{ r }}Controller(mockService)
	createReq := models.Create{{ r }}Request{Name: ""}
	body, _ := json.Marshal(createReq)

	result, err := controller.Create(context.Background(), bytes.NewReader(body))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}

func TestCreate{{ r }}_ReturnsValidationError_WhenBodyIsInvalid(t *testing.T) {
	mockService := new(Mock{{ r }}Service)
	controller := newTest{{ r }}Controller(mockService)

	result, err := controller.Create(context.Background(), bytes.NewReader([]byte("invalid json")))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}
{%- endif %}
{%- if "update" in resource.operations %}

func TestUpdate{{ r }}_ReturnsUpdated{{ r }}Dto(t *testing.T) {
	mockService := new(Mock{{ r }}Service)
	controller := newTest{{ r }}Controller(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	updateReq := models.Update{{ r }}Request{Name: "mockUpdated{{ r }}", UpdatedBy: "TestUser"}
	expectedReq := models.Update{{ r }}Request{Id: id, Name: "mockUpdated{{ r }}", UpdatedBy: "TestUser"}
	expected := &models.{{ r }}Dto{Id: id, Name: "mockUpdated{{ r }}"}
	mockService.On("Update", mock.Anything, expectedReq).Return(expected, nil)

	body, _ := json.Marshal(updateReq)

	result, err := controller.Update(context.Background(), id, bytes.NewReader(body))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Update", mock.Anything, expectedReq)
}
{%- endif %}
{%- if "replace" in resource.operations %}

func TestReplace{{ r }}_ReturnsReplaced{{ r }}Dto(t *testing.T) {
	mockService := new(Mock{{ r }}Service)
	controller := newTest{{ r }}Controller(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	replaceReq := models.Replace{{ r }}Request{Name: "mockReplaced{{ r }}", UpdatedBy: "TestUser"}
	expectedReq := models.Replace{{ r }}Request{Id: id, Name: "mockReplaced{{ r }}", UpdatedBy: "TestUser"}
	expected := &models.{{ r }}Dto{Id: id, Name: "mockReplaced{{ r }}"}
	mockService.On("Replace", mock.Anything, expectedReq).Return(expected, nil)

	body, _ := json.Marshal(replaceReq)

	result, err := controller.Replace(context.Background(), id, bytes.NewReader(body))

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockService.AssertCalled(t, "Replace", mock.Anything, expectedReq)
}

func TestReplace{{ r }}_ReturnsValidationError_WhenNameIsEmpty(t *testing.T) {
	mockService := new(Mock{{ r }}Service)
	controller := newTest{{ r }}Controller(mockService)
	replaceReq := models.Replace{{ r }}Request{Name: ""}
	body, _ := json.Marshal(replaceReq)

	result, err := controller.Replace(context.Background(), "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", bytes.NewReader(body))

	assert.Nil(t, result)
	assert.Error(t, err)
	assert.IsType(t, &models.ValidationError{}, err)
}
{%- endif %}
{%- if "delete" in resource.operations %}

func TestDelete{{ r }}_CallsDeleteOnService(t *testing.T) {
	mockService := new(Mock{{ r }}Service)
	controller := newTest{{ r }}Controller(mockService)
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockService.On("Delete", mock.Anything, id).Return(nil)

	err := controller.Delete(context.Background(), id)

	assert.NoError(t, err)
	mockService.AssertCalled(t, "Delete", mock.Anything, id)
}
{%- endif %}
{% endfor %}