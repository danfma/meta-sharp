import { defineConfig } from "vite";
import viteTsConfigPath from "vite-tsconfig-paths";

export default defineConfig({
  plugins: [viteTsConfigPath()],
});
