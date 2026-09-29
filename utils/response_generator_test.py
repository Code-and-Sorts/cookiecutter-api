from models import CatResponse
from .response_generator import response_generator

def describe_response_generator():
    def test_list_item_response_single_item():
        item_response = CatResponse(id="935e5045-4a1c-46c9-8e26-9d9d5c2597f3",name="mockName",type="mockType")
        item_response_list = [item_response]
        response = response_generator(item_response_list, 200)
        assert response["body"] == '[{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}]'
        assert response["statusCode"] == 200

    def test_empty_list():
        item_response_list = []
        response = response_generator(item_response_list, 200)
        assert response["body"] == '[]'
        assert response["statusCode"] == 200

    def test_list_item_response_multiple_items():
        item_response = CatResponse(id="935e5045-4a1c-46c9-8e26-9d9d5c2597f3",name="mockName",type="mockType")
        item_response_list = [item_response, item_response]
        response = response_generator(item_response_list, 200)
        assert response["body"] == '[{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}, {"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}]'
        assert response["statusCode"] == 200

    def test_item_response():
        item_response = CatResponse(id="935e5045-4a1c-46c9-8e26-9d9d5c2597f3",name="mockName",type="mockType")
        response = response_generator(item_response, 200)
        assert response["body"] == '{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}'
        assert response["statusCode"] == 200
