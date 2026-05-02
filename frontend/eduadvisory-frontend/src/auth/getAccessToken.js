import keycloak from "../keycloak";

export async function getAccessToken() {
  if (!keycloak.authenticated) {
    throw new Error("User is not authenticated");
  }

  await keycloak.updateToken(30);

  return keycloak.token;
}