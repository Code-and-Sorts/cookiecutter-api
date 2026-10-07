from copier_config import default, write

tools = default("infra_tools")
write(
    "GITHUB_ENV",
    {
        "TERRAFORM_VERSION": tools["terraform"],
        "ATMOS_VERSION": tools["atmos"],
        "UV_VERSION": default("runtime_versions")["uv"],
    },
)
