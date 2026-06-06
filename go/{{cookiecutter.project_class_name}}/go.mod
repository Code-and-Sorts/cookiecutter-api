module {{cookiecutter.project_endpoint}}

go 1.25

require (
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
	github.com/Azure/azure-sdk-for-go/sdk/azcore v1.22.0
	github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos v1.4.2
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' %}
	cloud.google.com/go/firestore v1.22.0
	google.golang.org/api v0.283.0
	google.golang.org/grpc v1.81.1
{%- elif cookiecutter.cloud_service == 'AWS Lambda' %}
	github.com/aws/aws-lambda-go v1.54.0
	github.com/aws/aws-sdk-go-v2 v1.41.12
	github.com/aws/aws-sdk-go-v2/config v1.32.23
	github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue v1.20.46
	github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression v1.8.46
	github.com/aws/aws-sdk-go-v2/service/dynamodb v1.58.0
{%- endif %}
	github.com/google/uuid v1.6.0
	github.com/santhosh-tekuri/jsonschema/v6 v6.0.2
	github.com/stretchr/testify v1.11.1
)
