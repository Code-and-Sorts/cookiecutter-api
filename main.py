"""
Main entry point for GCP Cloud Functions.
Each function is exported individually for GCP deployment.
"""
import functions_framework
from blueprints.api import (
    get_list_kitten_claws,
    get_by_id_kitten_claws,
    create_kitten_claws,
    update_kitten_claws,
    delete_kitten_claws,
    health
)

# Export functions for GCP Cloud Functions
# These can be deployed individually as separate cloud functions
__all__ = [
    'get_list_kitten_claws',
    'get_by_id_kitten_claws',
    'create_kitten_claws',
    'update_kitten_claws',
    'delete_kitten_claws',
    'health'
]
