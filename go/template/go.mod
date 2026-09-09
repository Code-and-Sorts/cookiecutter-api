module {{project_endpoint}}

go 1.25

require (
{%- if cloud_service == 'Azure Function App' %}
	github.com/Azure/azure-sdk-for-go/sdk/azcore v1.23.1
	github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos v1.5.0
{%- elif cloud_service == 'GCP Cloud Function' %}
	cloud.google.com/go/firestore v1.25.0
	google.golang.org/api v0.297.0
	google.golang.org/grpc v1.83.2
{%- elif cloud_service == 'AWS Lambda' %}
	github.com/aws/aws-lambda-go v1.55.0
	github.com/aws/aws-sdk-go-v2 v1.47.0
	github.com/aws/aws-sdk-go-v2/config v1.33.4
	github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue v1.21.4
	github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression v1.9.4
	github.com/aws/aws-sdk-go-v2/service/dynamodb v1.68.0
{%- endif %}
	github.com/google/uuid v1.6.0
	github.com/santhosh-tekuri/jsonschema/v6 v6.0.3
	github.com/stretchr/testify v1.12.1
)
