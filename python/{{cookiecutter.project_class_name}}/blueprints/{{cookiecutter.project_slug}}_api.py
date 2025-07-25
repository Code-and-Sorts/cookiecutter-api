{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import os
import logging
import azure.functions as func
from azure.cosmos import CosmosClient
from controllers import {{ cookiecutter.project_class_name }}Controller
from services import {{ cookiecutter.project_class_name }}Service
from repositories import {{ cookiecutter.project_class_name }}Repository, Database
from utils import detect_error, response_generator

bp = func.Blueprint()

database = Database(
    endpoint=os.getenv("Cosmos_Db_Uri"),
    key=os.getenv("Cosmos_Db_Key"),
    name=os.getenv("Cosmos_Db_Database_Name"),
    container_name=os.getenv("Cosmos_Db_Container_Name")
)
client = CosmosClient(database._endpoint, database._key)
database_client = client.get_database_client(database.name)
container_client = database_client.get_container_client(database.container_name)
repository = {{ cookiecutter.project_class_name }}Repository(container_client)
service = {{ cookiecutter.project_class_name }}Service(repository)
controller = {{ cookiecutter.project_class_name }}Controller(service)

@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.GET])
async def get_by_id(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get {{ cookiecutter.project_endpoint }} by ID processed a request.")

    try:
        item = controller.get_by_id(req)
        return response_generator(item)

    except Exception as error:
        return detect_error(error)

@bp.route(route="{{ cookiecutter.project_endpoint }}", methods=[func.HttpMethod.GET])
async def get_list(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Get {{ cookiecutter.project_endpoint }} list processed a request.")

    try:
        items = controller.get_list()
        return response_generator(items)

    except Exception as error:
        return detect_error(error)

@bp.route(route="{{ cookiecutter.project_endpoint }}", methods=[func.HttpMethod.POST])
async def create(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Create item processed a request.")

    try:
        created_item = controller.create(req)
        return response_generator(created_item, 201)

    except Exception as error:
        return detect_error(error)

@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.PATCH])
async def update(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Patch item processed a request.")

    try:
        updated_item = controller.update(req)
        return response_generator(updated_item, 201)

    except Exception as error:
        return detect_error(error)

@bp.route(route="{{ cookiecutter.project_endpoint }}/{item_id}", methods=[func.HttpMethod.DELETE])
async def delete(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Delete item processed a request.")

    try:
        controller.soft_delete(req)
        return func.HttpResponse(
            body="{{ cookiecutter.project_class_name }} deleted.",
            status_code=200
        )

    except Exception as error:
        return detect_error(error)
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
import os
import logging
import functions_framework
from flask import jsonify, request, Flask
from google.cloud import firestore
from controllers import {{ cookiecutter.project_class_name }}Controller
from services import {{ cookiecutter.project_class_name }}Service
from repositories import {{ cookiecutter.project_class_name }}Repository, Database
from utils import detect_error, response_generator_gcp

# Initialize Firestore client
db = firestore.Client()
collection_name = os.getenv("FIRESTORE_COLLECTION_NAME", "{{ cookiecutter.project_slug }}s")

repository = {{ cookiecutter.project_class_name }}Repository(db, collection_name)
service = {{ cookiecutter.project_class_name }}Service(repository)
controller = {{ cookiecutter.project_class_name }}Controller(service)

app = Flask(__name__)

@functions_framework.http
def {{ cookiecutter.project_slug }}_api(request):
    """HTTP Cloud Function that handles all {{ cookiecutter.project_name }} API requests."""
    
    # Enable CORS
    if request.method == 'OPTIONS':
        headers = {
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Methods': 'GET, POST, PATCH, DELETE',
            'Access-Control-Allow-Headers': 'Content-Type',
            'Access-Control-Max-Age': '3600'
        }
        return ('', 204, headers)

    headers = {'Access-Control-Allow-Origin': '*'}
    
    try:
        # Parse the request path
        path = request.path
        method = request.method
        
        # Handle different routes
        if path == '/{{ cookiecutter.project_endpoint }}' or path == '/':
            if method == 'GET':
                # Get list of items
                logging.info("Get {{ cookiecutter.project_endpoint }} list processed a request.")
                items = controller.get_list()
                return response_generator_gcp(items, headers=headers)
            
            elif method == 'POST':
                # Create new item
                logging.info("Create item processed a request.")
                item_json = request.get_json()
                created_item = controller.create(item_json)
                return response_generator_gcp(created_item, status_code=201, headers=headers)
        
        elif path.startswith('/{{ cookiecutter.project_endpoint }}/'):
            # Extract item_id from path
            item_id = path.split('/')[-1]
            
            if method == 'GET':
                # Get item by ID
                logging.info("Get {{ cookiecutter.project_endpoint }} by ID processed a request.")
                item = controller.get_by_id(item_id)
                return response_generator_gcp(item, headers=headers)
            
            elif method == 'PATCH':
                # Update item
                logging.info("Patch item processed a request.")
                item_data = request.get_json()
                updated_item = controller.update(item_id, item_data)
                return response_generator_gcp(updated_item, status_code=201, headers=headers)
            
            elif method == 'DELETE':
                # Delete item
                logging.info("Delete item processed a request.")
                controller.soft_delete(item_id)
                return (jsonify({"message": "{{ cookiecutter.project_class_name }} deleted."}), 200, headers)
        
        # Route not found
        return (jsonify({"error": "Route not found"}), 404, headers)
        
    except Exception as error:
        return detect_error(error, headers)
{%- endif %}
