import { useAtom } from "jotai";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  environmentsAtom,
  selectedEnvironmentIdAtom,
  selectedRouteIdAtom,
  selectedResponseIdAtom,
} from "../../state/atoms";

const HTTP_STATUS_CODES = [
  { code: "200", name: "OK" },
  { code: "201", name: "Created" },
  { code: "204", name: "No Content" },
  { code: "400", name: "Bad Request" },
  { code: "401", name: "Unauthorized" },
  { code: "403", name: "Forbidden" },
  { code: "404", name: "Not Found" },
  { code: "500", name: "Internal Server Error" },
];

export function ResponseConfig() {
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
    return <div>Select a response to configure</div>;
  }

  const updateResponse = (updates: Partial<typeof selectedResponse>) => {
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
                          ? { ...response, ...updates }
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

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="status-code">Status Code</Label>
        <Select
          value={selectedResponse.statusCode}
          onValueChange={(value) => updateResponse({ statusCode: value })}
        >
          <SelectTrigger className="w-[200px]">
            <SelectValue placeholder="Select status code" />
          </SelectTrigger>
          <SelectContent>
            {HTTP_STATUS_CODES.map((status) => (
              <SelectItem key={status.code} value={status.code}>
                {status.code} - {status.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="space-y-2">
        <Label htmlFor="response-body">Response Body</Label>
        <Textarea
          id="response-body"
          value={selectedResponse.body}
          onChange={(e) => updateResponse({ body: e.target.value })}
          placeholder="Enter JSON response"
          rows={10}
        />
      </div>
      <Button>Save Response</Button>
    </div>
  );
}
