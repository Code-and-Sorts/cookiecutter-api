"""
Main entry point for GCP Cloud Functions.
Each function is exported individually for GCP deployment.
"""
import functions_framework
from blueprints.{{cookiecutter.project_slug}}_api import (
    get_by_id,
    get_list,
    create,
    update,
    delete
)

# Export functions for GCP Cloud Functions
# These can be deployed individually as separate cloud functions
__all__ = ['get_by_id', 'get_list', 'create', 'update', 'delete']
