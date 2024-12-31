!#/bin/bash
# hcp auth login
# hcp profile init --vault-secrets
# hcp vault-secrets secrets open {desired secret}
# hcp vault-secrets run -- python3 my_app.py
# Afficher les organisations
## hcp organizations list
## Récupérer l'id de l'organisation

## curl --location "https://api.cloud.hashicorp.com/secrets/2023-11-28/organizations" --request GET --header
# Lister les projets présents dans l'organisation
## hcp projects list 
## Récupérer l'id du projet
## curl --location "https://api.cloud.hashicorp.com/secrets/2023-11-28/organizations/{organization_id}/projects" --request GET --header
# Lister les applications présentes dans le projet
## hcp apps list
## Récupérer l'id de l'application
## curl --location "https://api.cloud.hashicorp.com/secrets/2023-11-28/organizations/{organization_id}/projects/{project_id}/apps" --request GET --header
# Lister les secrets de l'application
## hcp secrets list
## Récupérer l'id du secret
## curl --location "https://api.cloud.hashicorp.com/secrets/2023-11-28/organizations/{organization_id}/projects/{project_id}/apps/{app_id}/secrets" --request GET --header
# Récupérer le secret
## hcp secrets open {secret_id}
## curl --location "https://api.cloud.hashicorp.com/secrets/2023-11-28/organizations/{organization_id}/projects/{project_id}/apps/{app_id}/secrets:{secret_id}" --request GET --header

curl \
--location "https://api.cloud.hashicorp.com/secrets/2023-11-28/organizations/5ca76712-a888-4902-8be7-d7d9063807e8/projects/2791f98d-681b-442d-ad5e-a88f3c4d000a/apps/Authentication-service/secrets:open" \
--request GET \
--header "Authorization: Bearer $HCP_API_TOKEN" | jq