from models import ItemResponse
from .response_generator import response_generator

# Test response generator with a list item with single ItemResponse
def test_list_item_response_single_item_response_generator():
    item_response = ItemResponse(id="mockId",name="mockName",type="mockType")
    item_response_list = [item_response]
    response = response_generator(item_response_list, 200)

    assert response.get_body().decode() == '[{"name": "mockName", "type": "mockType", "id": "mockId"}]'
    assert response.status_code == 200

# Test response generator with an empty list
def test_empty_list_response_generator():
    item_response_list = []
    response = response_generator(item_response_list, 200)

    assert response.get_body().decode() == '[]'
    assert response.status_code == 200

# Test response generator with a list item with multiple ItemResponses
def test_list_item_response_multiple_items_response_generator():
    item_response = ItemResponse(id="mockId",name="mockName",type="mockType")
    item_response_list = [item_response, item_response]
    response = response_generator(item_response_list, 200)

    assert response.get_body().decode() == '[{"name": "mockName", "type": "mockType", "id": "mockId"}, {"name": "mockName", "type": "mockType", "id": "mockId"}]'
    assert response.status_code == 200

# Test response generator with an ItemResponse
def test_item_response_response_generator():
    item_response = ItemResponse(id="mockId",name="mockName",type="mockType")
    response = response_generator(item_response, 200)

    assert response.get_body().decode() == '{"name": "mockName", "type": "mockType", "id": "mockId"}'
    assert response.status_code == 200
