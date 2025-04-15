import { useAtom } from "jotai";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../components/ui/select";
import { Ban, Plus, X } from "lucide-react";
import {
  environmentsAtom,
  selectedEnvironmentIdAtom,
  selectedRouteIdAtom,
  selectedResponseIdAtom,
  RuleGroup,
  SimpleRule,
  Rule,
} from "../../state/atoms";

const CONDITION_OPTIONS = [
  { value: "AND", label: "AND" },
  { value: "OR", label: "OR" },
];

const FIELD_OPTIONS = [
  { value: "Header", label: "Header" },
  { value: "Query", label: "Query" },
  { value: "Route", label: "Route" },
  { value: "Cookie", label: "Cookie" },
  { value: "Body", label: "Body" },
];

const OPERATION_OPTIONS = [
  { value: "Equals", label: "Equals" },
  { value: "Contains", label: "Contains" },
  { value: "StartsWith", label: "Starts With" },
  { value: "EndsWith", label: "Ends With" },
];

const findRuleAtPath = (rules: Rule[], path: string[]): Rule | undefined => {
  if (path.length === 0) return undefined;

  const [currentId, ...restPath] = path;
  const currentRule = rules.find((rule) => rule.id === currentId);

  if (!currentRule) return undefined;
  if (restPath.length === 0) return currentRule;

  if (currentRule.type === "Aggregate") {
    return findRuleAtPath(currentRule.rules, restPath);
  }

  return undefined;
};

const updateRuleAtPath = (
  rules: Rule[],
  path: string[],
  updateFn: (rule: Rule) => Rule
): Rule[] => {
  if (path.length === 0) {
    return rules.map(updateFn);
  }

  const [currentId, ...restPath] = path;

  return rules.map((rule) => {
    if (rule.id !== currentId) return rule;

    if (rule.type === "Aggregate" && restPath.length > 0) {
      return {
        ...rule,
        rules: updateRuleAtPath(rule.rules, restPath, updateFn),
      };
    }

    return updateFn(rule);
  });
};

export function RulesEditor() {
  const [environments, setEnvironments] = useAtom(environmentsAtom);
  const [selectedEnvironmentId] = useAtom(selectedEnvironmentIdAtom);
  const [selectedRouteId] = useAtom(selectedRouteIdAtom);
  const [selectedResponseId] = useAtom(selectedResponseIdAtom);

  const selectedEnvironment = environments.find(
    (env) => env.id === selectedEnvironmentId
  );
  const selectedRoute = selectedEnvironment?.routes.find(
    (route) => route.id === selectedRouteId
  );
  const selectedResponse = selectedRoute?.responses.find(
    (response) => response.id === selectedResponseId
  );

  if (!selectedResponse) {
    return <div>Select a response to configure rules</div>;
  }

  const updateResponseRules = (newRules: Rule[]) => {
    setEnvironments(
      environments.map((env) =>
        env.id === selectedEnvironmentId
          ? {
              ...env,
              routes: env.routes.map((route) =>
                route.id === selectedRouteId
                  ? {
                      ...route,
                      responses: route.responses.map((response) =>
                        response.id === selectedResponseId
                          ? { ...response, rules: newRules }
                          : response
                      ),
                    }
                  : route
              ),
            }
          : env
      )
    );
  };

  const addRuleGroup = () => {
    const newGroup: RuleGroup = {
      type: "Aggregate",
      id: Date.now().toString(),
      op: "AND",
      rules: [],
    };
    updateResponseRules([...selectedResponse.rules, newGroup]);
  };

  // Add a simple rule at a specific path
  const addSimpleRule = (path: string[] = []) => {
    const newRule: SimpleRule = {
      id: Date.now().toString(),
      type: "Header",
      key: "",
      operation: "Equals",
      value: "",
      negate: false,
    };

    if (path.length === 0) {
      // Add to root
      updateResponseRules([...selectedResponse.rules, newRule]);
      return;
    }

    // Find the parent group
    const parentGroup = findRuleAtPath(selectedResponse.rules, path);
    if (!parentGroup || parentGroup.type !== "Aggregate") return;

    // Update the rules by adding the new rule to the parent group
    const updatedRules = updateRuleAtPath(
      selectedResponse.rules,
      path,
      (rule) => {
        if (rule.type !== "Aggregate") return rule;
        return {
          ...rule,
          rules: [...rule.rules, newRule],
        };
      }
    );

    updateResponseRules(updatedRules);
  };

  // Add a nested group at a specific path
  const addNestedGroup = (path: string[]) => {
    if (path.length === 0) return;

    const newGroup: RuleGroup = {
      type: "Aggregate",
      id: Date.now().toString(),
      op: "AND",
      rules: [],
    };

    // Update the rules by adding the new group to the parent group
    const updatedRules = updateRuleAtPath(
      selectedResponse.rules,
      path,
      (rule) => {
        if (rule.type !== "Aggregate") return rule;
        return {
          ...rule,
          rules: [...rule.rules, newGroup],
        };
      }
    );

    updateResponseRules(updatedRules);
  };

  // Update a simple rule at a specific path
  const updateSimpleRule = (
    path: string[],
    field: keyof SimpleRule,
    value: any
  ) => {
    if (path.length === 0) return;

    const updatedRules = updateRuleAtPath(
      selectedResponse.rules,
      path,
      (rule) => {
        if (rule.type !== "Aggregate") return rule;
        return {
          ...rule,
          [field]: value,
        };
      }
    );

    updateResponseRules(updatedRules);
  };

  // Update a group's condition at a specific path
  const updateGroupCondition = (path: string[], condition: "AND" | "OR") => {
    if (path.length === 0) return;

    const updatedRules = updateRuleAtPath(
      selectedResponse.rules,
      path,
      (rule) => {
        if (rule.type !== "Aggregate") return rule;
        return {
          ...rule,
          condition,
        };
      }
    );

    updateResponseRules(updatedRules);
  };

  // Delete a rule at a specific path
  const deleteRule = (path: string[]) => {
    if (path.length === 0) return;

    const [parentPath, ruleId] = [path.slice(0, -1), path[path.length - 1]];

    if (parentPath.length === 0) {
      // Delete from root
      updateResponseRules(
        selectedResponse.rules.filter((rule) => rule.id !== ruleId)
      );
      return;
    }

    // Delete from a parent group
    const updatedRules = updateRuleAtPath(
      selectedResponse.rules,
      parentPath,
      (rule) => {
        if (rule.type !== "Aggregate") return rule;
        return {
          ...rule,
          rules: rule.rules.filter((r) => r.id !== ruleId),
        };
      }
    );

    updateResponseRules(updatedRules);
  };
  console.log(selectedResponse.rules);
  const renderSimpleRule = (rule: SimpleRule, path: string[]) => (
    <div key={rule.id} className="flex items-center space-x-2 ml-6 my-2">
      <Select
        value={rule.type}
        onValueChange={(value: string) =>
          updateSimpleRule([...path, rule.id], "key", value)
        }
      >
        <SelectTrigger className="min-w-[120px] w-[120px] max-w-[120px]">
          <SelectValue placeholder="Field" />
        </SelectTrigger>
        <SelectContent>
          {FIELD_OPTIONS.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <Input
        placeholder="Key Name"
        value={rule.key}
        onChange={(e) =>
          updateSimpleRule([...path, rule.id], "key", e.target.value)
        }
        className="flex-1"
      />

      <Button
        variant={rule.negate ? "default" : "outline"}
        size="icon"
        onClick={() =>
          updateSimpleRule([...path, rule.id], "negate", !rule.negate)
        }
        title="Negate"
      >
        <Ban className="h-4 w-4" />
      </Button>

      <Select
        value={rule.operation}
        onValueChange={(value: string) =>
          updateSimpleRule([...path, rule.id], "operation", value)
        }
      >
        <SelectTrigger className="min-w-[120px] w-[120px] max-w-[120px]">
          <SelectValue placeholder="Operation" />
        </SelectTrigger>
        <SelectContent>
          {OPERATION_OPTIONS.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Input
        placeholder="Value"
        value={rule.value}
        onChange={(e) =>
          updateSimpleRule([...path, rule.id], "value", e.target.value)
        }
        className="flex-1"
      />

      <Button
        variant="ghost"
        size="icon"
        onClick={() => deleteRule([...path, rule.id])}
        title="Delete Rule"
      >
        <X className="h-4 w-4" />
      </Button>
    </div>
  );

  const renderRuleGroup = (group: RuleGroup, path: string[]) => (
    <div key={group.id} className="border rounded-md p-4 my-2">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center space-x-2">
          <Select
            value={group.op}
            onValueChange={(value: "AND" | "OR") =>
              updateGroupCondition([...path, group.id], value)
            }
          >
            <SelectTrigger className="w-[100px]">
              <SelectValue placeholder="Condition" />
            </SelectTrigger>
            <SelectContent>
              {CONDITION_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="flex items-center space-x-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => addSimpleRule([...path, group.id])}
          >
            + Rule
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => addNestedGroup([...path, group.id])}
          >
            + Group
          </Button>

          <Button
            variant="ghost"
            size="icon"
            onClick={() => deleteRule([...path, group.id])}
            title="Delete Group"
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      </div>

      <div className="pl-4 border-l-2 border-gray-200">
        {group.rules.map((rule) =>
          rule.type !== "Aggregate"
            ? renderSimpleRule(rule, [...path, group.id])
            : renderRuleGroup(rule, [...path, group.id])
        )}
      </div>

      {group.rules.length === 0 && (
        <div className="text-muted-foreground text-sm pl-6">
          No rules in this group. Add a rule or group.
        </div>
      )}
    </div>
  );

  return (
    <div className="space-y-4">
      {selectedResponse.rules.map((rule) =>
        rule.type !== "Aggregate"
          ? renderSimpleRule(rule, [])
          : renderRuleGroup(rule, [])
      )}

      <div className="flex space-x-2">
        <Button variant="outline" onClick={() => addSimpleRule()}>
          <Plus className="mr-2 h-4 w-4" /> Add Rule
        </Button>

        <Button variant="outline" onClick={addRuleGroup}>
          <Plus className="mr-2 h-4 w-4" /> Add Group
        </Button>
      </div>
    </div>
  );
}
