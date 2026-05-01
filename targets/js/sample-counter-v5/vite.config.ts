import { defineConfig } from "vite";
import viteTsConfigPath from "vite-tsconfig-paths";
import solid from "vite-plugin-solid";

export default defineConfig({
  plugins: [viteTsConfigPath(), solid()],
});
