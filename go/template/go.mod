module {{project_endpoint}}

go {{ runtime.go }}

require (
{%- if cloud_service == 'Azure Function App' %}
	github.com/Azure/azure-sdk-for-go/sdk/azcore v1.23.3
	github.com/Azure/azure-sdk-for-go/sdk/azidentity v1.14.1
	github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos v1.5.0
{%- elif cloud_service == 'GCP Cloud Function' %}
	cloud.google.com/go/firestore v1.26.0
	github.com/GoogleCloudPlatform/functions-framework-go v1.9.2
	google.golang.org/api v0.300.0
	google.golang.org/grpc v1.84.0
{%- elif cloud_service == 'AWS Lambda' %}
	github.com/aws/aws-lambda-go v1.55.1
	github.com/aws/aws-sdk-go-v2 v1.47.1
	github.com/aws/aws-sdk-go-v2/config v1.33.7
	github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue v1.21.9
	github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression v1.9.9
	github.com/aws/aws-sdk-go-v2/service/dynamodb v1.70.1
{%- endif %}
	github.com/google/uuid v1.6.0
	github.com/santhosh-tekuri/jsonschema/v6 v6.0.3
	github.com/stretchr/testify v1.12.1
	golang.org/x/text v0.42.0
)
