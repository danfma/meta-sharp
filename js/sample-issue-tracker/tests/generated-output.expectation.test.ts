import { describe, expect, test } from "bun:test";
import { existsSync } from "node:fs";
import { fileURLToPath } from "node:url";

function generatedPath(relativePath: string): string {
  return fileURLToPath(new URL(`../src/${relativePath}`, import.meta.url));
}

async function readGenerated(relativePath: string): Promise<string> {
  const path = generatedPath(relativePath);
  expect(existsSync(path)).toBe(true);
  return Bun.file(path).text();
}

describe("SampleIssueTracker generated output", () => {
  test("emits the expected feature slices and key domain files", () => {
    const expectedFiles = [
      "index.ts",
      "Issues/Domain/Issue.ts",
      "Issues/Domain/IssueStatus.ts",
      "Issues/Domain/IssuePriority.ts",
      "Issues/Domain/IssueType.ts",
      "Issues/Domain/IssueId.ts",
      "Issues/Application/IssueQueries.ts",
      "Issues/Application/IssueService.ts",
      "SharedKernel/UserId.ts",
      "SharedKernel/PageRequest.ts",
      "Planning/Domain/Sprint.ts",
    ];

    for (const relativePath of expectedFiles) {
      expect(existsSync(generatedPath(relativePath))).toBe(true);
    }
  });

  test("exports issue queries as top-level module functions", async () => {
    const source = await readGenerated("Issues/Application/IssueQueries.ts");

    expect(source).toContain("export function openIssues");
    expect(source).toContain("export function statusCounts");
    expect(source).not.toContain("class IssueQueries");
  });

  test("does not emit unsupported placeholders in the core issue slice", async () => {
    const issueSource = await readGenerated("Issues/Domain/Issue.ts");
    const serviceSource = await readGenerated("Issues/Application/IssueService.ts");
    const querySource = await readGenerated("Issues/Application/IssueQueries.ts");

    expect(issueSource).not.toContain("unsupported:");
    expect(serviceSource).not.toContain("unsupported:");
    expect(querySource).not.toContain("unsupported:");
  });

  test("keeps generated references bound to real exported symbols", async () => {
    const issueSource = await readGenerated("Issues/Domain/Issue.ts");
    const commentSource = await readGenerated("Issues/Domain/Comment.ts");
    const issueIdSource = await readGenerated("Issues/Domain/IssueId.ts");
    const userIdSource = await readGenerated("SharedKernel/UserId.ts");
    const serviceSource = await readGenerated("Issues/Application/IssueService.ts");
    const querySource = await readGenerated("Issues/Application/IssueQueries.ts");

    const unresolvedPatterns = [
      /\bissueStatus\./,
      /\bissuePriority\./,
      /\bissueWorkflow\./,
      /\bdateTimeOffset\./,
      /\bcomment\./,
      /\buserId\./,
      /\bissueId\./,
      /\bguid\./,
      /\brepository\b/,
      /\bIGrouping\b/,
    ];

    for (const pattern of unresolvedPatterns) {
      expect(issueSource).not.toMatch(pattern);
      expect(commentSource).not.toMatch(pattern);
      expect(issueIdSource).not.toMatch(pattern);
      expect(userIdSource).not.toMatch(pattern);
      expect(serviceSource).not.toMatch(pattern);
      expect(querySource).not.toMatch(pattern);
    }
  });

  test("emits async overload dispatchers as async methods when awaits are present", async () => {
    const serviceSource = await readGenerated("Issues/Application/IssueService.ts");

    expect(serviceSource).toContain("async createAsync");
    expect(serviceSource).toContain("async addCommentAsync");
  });
});
