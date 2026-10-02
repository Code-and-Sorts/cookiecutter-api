import azure.functions as func
from blueprints import BLUEPRINTS

# Resource functions require a function key; the health check opts out.
app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

for blueprint in BLUEPRINTS:
    app.register_functions(blueprint)
