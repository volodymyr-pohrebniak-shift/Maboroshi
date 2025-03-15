import { useAtom } from "jotai";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Ban } from "lucide-react";
import {
  environmentsAtom,
  selectedEnvironmentIdAtom,
  selectedRouteIdAtom,
  selectedResponseIdAtom,
} from "../../state/atoms";

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

  const updateRules = (newRules: typeof selectedResponse.rules) => {
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

  const addRule = () => {
    const newRule = {
      id: Date.now().toString(),
      type: "header",
      key: "",
      operation: "equals",
      value: "",
      negate: false,
    };
    updateRules([...selectedResponse.rules, newRule]);
  };

  const updateRule = (
    id: string,
    field: keyof (typeof selectedResponse.rules)[0],
    value: any
  ) => {
    updateRules(
      selectedResponse.rules.map((rule) =>
        rule.id === id ? { ...rule, [field]: value } : rule
      )
    );
  };

  return (
    <div className="space-y-4">
      {selectedResponse.rules.map((rule) => (
        <div key={rule.id} className="flex space-x-2 items-center">
          <Select
            value={rule.type}
            onValueChange={(value) => updateRule(rule.id, "type", value)}
          >
            <SelectTrigger className="w-[120px]">
              <SelectValue placeholder="Type" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="header">Header</SelectItem>
              <SelectItem value="query">Query</SelectItem>
              <SelectItem value="route">Route</SelectItem>
              <SelectItem value="cookie">Cookie</SelectItem>
              <SelectItem value="body">Body</SelectItem>
            </SelectContent>
          </Select>
          <Input
            placeholder="Key"
            value={rule.key}
            onChange={(e) => updateRule(rule.id, "key", e.target.value)}
          />
          <Button
            variant={rule.negate ? "default" : "outline"}
            size="icon"
            onClick={() => updateRule(rule.id, "negate", !rule.negate)}
            className="px-2"
          >
            <Ban className="h-4 w-4" />
          </Button>
          <Select
            value={rule.operation}
            onValueChange={(value) => updateRule(rule.id, "operation", value)}
          >
            <SelectTrigger className="w-[120px]">
              <SelectValue placeholder="Operation" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="equals">Equals</SelectItem>
              <SelectItem value="contains">Contains</SelectItem>
              <SelectItem value="startsWith">Starts with</SelectItem>
              <SelectItem value="endsWith">Ends with</SelectItem>
            </SelectContent>
          </Select>
          <Input
            placeholder="Value"
            value={rule.value}
            onChange={(e) => updateRule(rule.id, "value", e.target.value)}
          />
        </div>
      ))}
      <Button onClick={addRule}>Add Rule</Button>
    </div>
  );
}
