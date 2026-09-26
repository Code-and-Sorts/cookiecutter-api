"""
Main entry point for GCP Cloud Functions.
Each function is exported individually for GCP deployment.
"""
import functions_framework
from blueprints.api import (
    get_list_cat,
    get_by_id_cat,
    create_cat,
    update_cat,
    delete_cat,
    get_list_dog,
    get_by_id_dog,
    create_dog,
    replace_dog,
    delete_dog,
    health
)

# Export functions for GCP Cloud Functions
# These can be deployed individually as separate cloud functions
__all__ = [
    'get_list_cat',
    'get_by_id_cat',
    'create_cat',
    'update_cat',
    'delete_cat',
    'get_list_dog',
    'get_by_id_dog',
    'create_dog',
    'replace_dog',
    'delete_dog',
    'health'
]
