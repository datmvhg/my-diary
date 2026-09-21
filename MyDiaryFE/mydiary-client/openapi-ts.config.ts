import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
  input: "http://localhost:5224/openapi/v1.json",
  output: "src/api/generated",
});