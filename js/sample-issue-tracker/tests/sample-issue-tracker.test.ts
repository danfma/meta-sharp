import { describe, expect, test } from "bun:test";
import { OperationResult } from "../src/SharedKernel/OperationResult.ts";
import { PageRequest } from "../src/SharedKernel/PageRequest.ts";
import { PageResult } from "../src/SharedKernel/PageResult.ts";
import { UserId } from "../src/SharedKernel/UserId.ts";
import { IssueId } from "../src/Issues/Domain/IssueId.ts";
import { IssueStatus } from "../src/Issues/Domain/IssueStatus.ts";
import { IssuePriority } from "../src/Issues/Domain/IssuePriority.ts";
import { IssueType } from "../src/Issues/Domain/IssueType.ts";
import { Issue } from "../src/Issues/Domain/Issue.ts";
import { Comment } from "../src/Issues/Domain/Comment.ts";
import { IssueWorkflow } from "../src/Issues/Domain/IssueWorkflow.ts";
import { InMemoryIssueRepository } from "../src/Issues/Application/InMemoryIssueRepository.ts";
import { IssueService } from "../src/Issues/Application/IssueService.ts";
import {
  openIssues,
  statusCounts,
  issuesForAssignee,
  readyForReview,
} from "../src/Issues/Application/IssueQueries.ts";

// ─── InlineWrapper (UserId, IssueId) ──────────────────────────

describe("InlineWrapper", () => {
  test("UserId.create produces a branded string", () => {
    const id = UserId.create("alice");
    expect(typeof id).toBe("string");
    expect(id).toBe("alice" as any);
  });

  test("UserId.system returns 'system'", () => {
    expect(UserId.system()).toBe("system" as any);
  });

  test("UserId.new_ generates a uuid-like string", () => {
    const id = UserId.new_();
    expect(typeof id).toBe("string");
    expect((id as string).length).toBeGreaterThan(0);
    expect((id as string)).not.toContain("-");
  });

  test("IssueId.new_ generates unique values", () => {
    const a = IssueId.new_();
    const b = IssueId.new_();
    expect(a).not.toBe(b);
  });

  test("UserId is comparable with ===", () => {
    const a = UserId.create("alice");
    const b = UserId.create("alice");
    expect(a === b).toBe(true);
  });
});

// ─── Enum (StringEnum) ────────────────────────────────────────

describe("StringEnum", () => {
  test("IssueStatus values are accessible as object members", () => {
    expect(IssueStatus.Backlog).toBe("backlog");
    expect(IssueStatus.Ready).toBe("ready");
    expect(IssueStatus.InProgress).toBe("in-progress");
    expect(IssueStatus.Done).toBe("done");
  });

  test("IssuePriority values are accessible", () => {
    expect(IssuePriority.Low).toBe("low");
    expect(IssuePriority.Urgent).toBe("urgent");
  });

  test("IssueType values are accessible", () => {
    expect(IssueType.Story).toBe("story");
    expect(IssueType.Bug).toBe("bug");
  });
});

// ─── OperationResult ──────────────────────────────────────────

describe("OperationResult", () => {
  test("ok() creates successful result with value", () => {
    const result = OperationResult.ok(42);
    expect(result.success).toBe(true);
    expect(result.value).toBe(42);
    expect(result.hasValue).toBe(true);
  });

  test("fail() creates failure with error info", () => {
    const result = OperationResult.fail<number>("invalid", "Bad input");
    expect(result.success).toBe(false);
    expect(result.value).toBeNull();
    expect(result.errorCode).toBe("invalid");
    expect(result.errorMessage).toBe("Bad input");
    expect(result.hasValue).toBe(false);
  });

  test("equals() compares structurally", () => {
    const a = OperationResult.ok("hello");
    const b = OperationResult.ok("hello");
    expect(a.equals(b)).toBe(true);
  });

  test("with() creates a modified copy", () => {
    const a = OperationResult.ok(1);
    const b = a.with({ value: 2 });
    expect(a.value).toBe(1);
    expect(b.value).toBe(2);
    expect(b.success).toBe(true);
  });
});

// ─── PageRequest ──────────────────────────────────────────────

describe("PageRequest", () => {
  test("default values are 1 and 20", () => {
    const req = new PageRequest();
    expect(req.number).toBe(1);
    expect(req.size).toBe(20);
  });

  test("safeNumber clamps to minimum 1", () => {
    const req = new PageRequest(0, 10);
    expect(req.safeNumber).toBe(1);
  });

  test("safeSize clamps to minimum 1", () => {
    const req = new PageRequest(1, 0);
    expect(req.safeSize).toBe(1);
  });

  test("skip computes offset correctly", () => {
    expect(new PageRequest(1, 20).skip).toBe(0);
    expect(new PageRequest(2, 20).skip).toBe(20);
    expect(new PageRequest(3, 10).skip).toBe(20);
  });
});

// ─── IssueWorkflow ────────────────────────────────────────────

describe("IssueWorkflow", () => {
  test("getAllowedTransitions from Backlog", () => {
    const result = IssueWorkflow.getAllowedTransitions(IssueStatus.Backlog);
    expect(result).toContain(IssueStatus.Ready);
    expect(result).toContain(IssueStatus.Cancelled);
  });

  test("getAllowedTransitions from Ready", () => {
    const result = IssueWorkflow.getAllowedTransitions(IssueStatus.Ready);
    expect(result).toContain(IssueStatus.InProgress);
    expect(result).toContain(IssueStatus.Cancelled);
  });

  test("getAllowedTransitions from Done is empty", () => {
    expect(IssueWorkflow.getAllowedTransitions(IssueStatus.Done)).toEqual([]);
  });

  test("canTransition validates allowed transitions", () => {
    expect(IssueWorkflow.canTransition(IssueStatus.Backlog, IssueStatus.Ready)).toBe(true);
    expect(IssueWorkflow.canTransition(IssueStatus.Backlog, IssueStatus.Done)).toBe(false);
    expect(IssueWorkflow.canTransition(IssueStatus.Ready, IssueStatus.InProgress)).toBe(true);
    expect(IssueWorkflow.canTransition(IssueStatus.InProgress, IssueStatus.InReview)).toBe(true);
  });

  test("describeLane returns correct lane for status", () => {
    const issue = makeIssue({ status: IssueStatus.Backlog });
    expect(IssueWorkflow.describeLane(issue)).toBe("triage");

    issue.status = IssueStatus.InProgress;
    expect(IssueWorkflow.describeLane(issue)).toBe("building");

    issue.status = IssueStatus.Done;
    expect(IssueWorkflow.describeLane(issue)).toBe("done");
  });

  test("describeLane: ready + urgent → expedite", () => {
    const issue = makeIssue({ status: IssueStatus.Ready, priority: IssuePriority.Urgent });
    expect(IssueWorkflow.describeLane(issue)).toBe("expedite");
  });
});

// ─── Issue (domain entity) ────────────────────────────────────

describe("Issue", () => {
  test("starts in Backlog status", () => {
    const issue = makeIssue();
    expect(issue.status).toBe(IssueStatus.Backlog);
  });

  test("isClosed is true for Done/Cancelled", () => {
    const issue = makeIssue();
    expect(issue.isClosed).toBe(false);
    issue.status = IssueStatus.Done;
    expect(issue.isClosed).toBe(true);
    issue.status = IssueStatus.Cancelled;
    expect(issue.isClosed).toBe(true);
  });

  test("rename() changes title", () => {
    const issue = makeIssue();
    issue.rename("New title");
    expect(issue.title).toBe("New title");
  });

  test("changePriority() updates priority", () => {
    const issue = makeIssue();
    issue.changePriority(IssuePriority.Urgent);
    expect(issue.priority).toBe(IssuePriority.Urgent);
  });

  test("assignTo() / unassign() work correctly", () => {
    const issue = makeIssue();
    expect(issue.assigneeId).toBeNull();
    issue.assignTo(UserId.create("alice"));
    expect(issue.assigneeId).toBe("alice" as any);
    issue.unassign();
    expect(issue.assigneeId).toBeNull();
  });

  test("addComment with author + message", () => {
    const issue = makeIssue();
    issue.addComment(UserId.create("bob"), "Looks good");
    expect(issue.commentCount).toBe(1);
    expect(issue.comments[0]?.message).toBe("Looks good");
  });

  test("transitionTo() valid transition", () => {
    const issue = makeIssue();
    issue.transitionTo(IssueStatus.Ready, UserId.create("alice"));
    expect(issue.status).toBe(IssueStatus.Ready);
    // Should have a system comment about the transition
    expect(issue.commentCount).toBe(1);
  });

  test("transitionTo() invalid transition throws", () => {
    const issue = makeIssue();
    expect(() => {
      issue.transitionTo(IssueStatus.Done, UserId.create("alice"));
    }).toThrow();
  });

  test("planForSprint() / removeFromSprint()", () => {
    const issue = makeIssue();
    issue.planForSprint("SPRINT-1");
    expect(issue.sprintKey).toBe("SPRINT-1");
    issue.removeFromSprint();
    expect(issue.sprintKey).toBeNull();
  });
});

// ─── Comment (record) ─────────────────────────────────────────

describe("Comment", () => {
  test("system() factory creates a system comment", () => {
    const c = Comment.system("Status changed", Temporal.Now.zonedDateTimeISO());
    expect(c.isSystem).toBe(true);
  });

  test("equals() compares structurally", () => {
    const ts = Temporal.Now.zonedDateTimeISO();
    const a = new Comment(UserId.create("alice"), "hi", ts);
    const b = new Comment(UserId.create("alice"), "hi", ts);
    expect(a.equals(b)).toBe(true);
  });
});

// ─── InMemoryIssueRepository ──────────────────────────────────

describe("InMemoryIssueRepository", () => {
  test("save and retrieve by id", async () => {
    const repo = new InMemoryIssueRepository();
    const issue = makeIssue();
    await repo.saveAsync(issue);
    const found = await repo.getByIdAsync(issue.id);
    expect(found).toBe(issue);
  });

  test("getByIdAsync returns null for missing id", async () => {
    const repo = new InMemoryIssueRepository();
    const found = await repo.getByIdAsync(IssueId.new_());
    expect(found).toBeNull();
  });

  test("listAsync returns saved issues", async () => {
    const repo = new InMemoryIssueRepository();
    const a = makeIssue({ title: "First" });
    const b = makeIssue({ title: "Second" });
    await repo.saveAsync(a);
    await repo.saveAsync(b);
    const list = await repo.listAsync();
    expect(list.length).toBe(2);
  });

  test("existsAsync", async () => {
    const repo = new InMemoryIssueRepository();
    const issue = makeIssue();
    expect(await repo.existsAsync(issue.id)).toBe(false);
    await repo.saveAsync(issue);
    expect(await repo.existsAsync(issue.id)).toBe(true);
  });

  test("listBySprintAsync filters by sprint", async () => {
    const repo = new InMemoryIssueRepository();
    const a = makeIssue();
    const b = makeIssue();
    a.planForSprint("SPRINT-1");
    b.planForSprint("SPRINT-2");
    await repo.saveAsync(a);
    await repo.saveAsync(b);
    const result = await repo.listBySprintAsync("SPRINT-1");
    expect(result.length).toBe(1);
    expect(result[0]?.id).toBe(a.id);
  });
});

// ─── IssueService ─────────────────────────────────────────────

describe("IssueService", () => {
  test("createAsync creates and saves a new issue", async () => {
    const repo = new InMemoryIssueRepository();
    const service = new IssueService(repo);
    const result = await service.createAsync("Test", "Desc", IssueType.Story, IssuePriority.Medium);
    expect(result.success).toBe(true);
    expect(result.value?.title).toBe("Test");
    expect(await repo.existsAsync(result.value!.id)).toBe(true);
  });

  test("createAsync with default priority", async () => {
    const repo = new InMemoryIssueRepository();
    const service = new IssueService(repo);
    const result = await service.createAsync("Test", "Desc", IssueType.Bug);
    expect(result.value?.priority).toBe(IssuePriority.Medium);
  });

  test("loadAsync returns failure for missing id", async () => {
    const repo = new InMemoryIssueRepository();
    const service = new IssueService(repo);
    const result = await service.loadAsync(IssueId.new_());
    expect(result.success).toBe(false);
    expect(result.errorCode).toBe("issue_not_found");
  });

  test("assignAsync sets assignee", async () => {
    const repo = new InMemoryIssueRepository();
    const service = new IssueService(repo);
    const created = await service.createAsync("Task", "x", IssueType.Story, IssuePriority.Low);
    const result = await service.assignAsync(created.value!.id, UserId.create("alice"));
    expect(result.value?.assigneeId).toBe("alice" as any);
  });

  test("transitionAsync moves through workflow", async () => {
    const repo = new InMemoryIssueRepository();
    const service = new IssueService(repo);
    const created = await service.createAsync("Task", "x", IssueType.Story, IssuePriority.Low);
    const result = await service.transitionAsync(
      created.value!.id,
      IssueStatus.Ready,
      UserId.create("alice")
    );
    expect(result.value?.status).toBe(IssueStatus.Ready);
  });
});

// ─── IssueQueries (LINQ exported as module) ───────────────────

describe("IssueQueries", () => {
  test("openIssues filters out closed issues", () => {
    const issues = [
      makeIssue({ title: "Open A", priority: IssuePriority.High }),
      makeIssue({ title: "Open B", priority: IssuePriority.Medium }),
      makeIssue({ title: "Closed", priority: IssuePriority.Urgent }),
    ];
    issues[2]!.status = IssueStatus.Done; // closed → filtered out

    const result = openIssues(issues);
    expect(result.length).toBe(2);
    expect(result.find(i => i.title === "Closed")).toBeUndefined();
  });

  test("openIssues orderByDescending then thenBy is applied", () => {
    // StringEnum compares alphabetically by value:
    // "low" < "medium" < "urgent" — so descending: urgent, medium, low
    const issues = [
      makeIssue({ title: "B", priority: IssuePriority.Low }),
      makeIssue({ title: "A", priority: IssuePriority.Urgent }),
      makeIssue({ title: "C", priority: IssuePriority.Urgent }),
    ];
    const result = openIssues(issues);
    expect(result.length).toBe(3);
    // Urgent first (alphabetically last in descending), then within same priority sort by title
    expect(result[0]?.priority).toBe(IssuePriority.Urgent);
    expect(result[1]?.priority).toBe(IssuePriority.Urgent);
    // Among urgents, A before C (thenBy title asc)
    expect(result[0]?.title).toBe("A");
    expect(result[1]?.title).toBe("C");
    // Then Low
    expect(result[2]?.priority).toBe(IssuePriority.Low);
  });

  test("statusCounts groups issues by status", () => {
    const issues = [
      makeIssue({ status: IssueStatus.Backlog }),
      makeIssue({ status: IssueStatus.Backlog }),
      makeIssue({ status: IssueStatus.Ready }),
    ];
    const counts = statusCounts(issues);
    expect(counts.get(IssueStatus.Backlog)).toBe(2);
    expect(counts.get(IssueStatus.Ready)).toBe(1);
  });

  test("issuesForAssignee filters by assignee", () => {
    const alice = UserId.create("alice");
    const bob = UserId.create("bob");
    const issues = [
      makeIssue({ assigneeId: alice }),
      makeIssue({ assigneeId: bob }),
      makeIssue({ assigneeId: alice }),
    ];
    expect(issuesForAssignee(issues, alice).length).toBe(2);
    expect(issuesForAssignee(issues, bob).length).toBe(1);
  });

  test("readyForReview returns high/urgent in-progress or in-review", () => {
    const issues = [
      makeIssue({ status: IssueStatus.InProgress, priority: IssuePriority.High }),
      makeIssue({ status: IssueStatus.InReview, priority: IssuePriority.Urgent }),
      makeIssue({ status: IssueStatus.InProgress, priority: IssuePriority.Low }), // filtered
      makeIssue({ status: IssueStatus.Backlog, priority: IssuePriority.Urgent }), // filtered
    ];
    const result = readyForReview(issues, 10);
    expect(result.length).toBe(2);
  });

  test("readyForReview respects limit", () => {
    const issues = [
      makeIssue({ status: IssueStatus.InProgress, priority: IssuePriority.High }),
      makeIssue({ status: IssueStatus.InProgress, priority: IssuePriority.High }),
      makeIssue({ status: IssueStatus.InProgress, priority: IssuePriority.High }),
    ];
    expect(readyForReview(issues, 2).length).toBe(2);
  });
});

// ─── PageResult ───────────────────────────────────────────────

describe("PageResult", () => {
  test("constructor stores items and metadata", () => {
    const items = [1, 2, 3];
    const page = new PageRequest(1, 10);
    const result = new PageResult(items, 50, page);
    expect(result.items).toBe(items);
    expect(result.totalCount).toBe(50);
    expect(result.page).toBe(page);
  });
});

// ─── helpers ──────────────────────────────────────────────────

import { Temporal } from "@js-temporal/polyfill";

function makeIssue(overrides?: Partial<{
  title: string;
  description: string;
  type: IssueType;
  priority: IssuePriority;
  status: IssueStatus;
  assigneeId: UserId;
}>): Issue {
  const issue = new Issue(
    IssueId.new_(),
    overrides?.title ?? "Test issue",
    overrides?.description ?? "Test description",
    overrides?.type ?? IssueType.Story,
    overrides?.priority ?? IssuePriority.Medium
  );
  if (overrides?.status !== undefined) issue.status = overrides.status;
  if (overrides?.assigneeId !== undefined) issue.assigneeId = overrides.assigneeId;
  return issue;
}
