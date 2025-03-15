import { useAtom } from "jotai";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Plus, X } from "lucide-react";
import {
  environmentsAtom,
  selectedEnvironmentIdAtom,
  selectedRouteIdAtom,
  selectedResponseIdAtom,
} from "../../state/atoms";

export function HeadersConfig() {
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
    return <div>Select a response to configure headers</div>;
  }

  const updateHeaders = (newHeaders: typeof selectedResponse.headers) => {
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
                          ? { ...response, headers: newHeaders }
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

  const addHeader = () => {
    const newHeader = { id: Date.now().toString(), key: "", value: "" };
    updateHeaders([...selectedResponse.headers, newHeader]);
  };

  const updateHeader = (id: string, field: "key" | "value", value: string) => {
    updateHeaders(
      selectedResponse.headers.map((header) =>
        header.id === id ? { ...header, [field]: value } : header
      )
    );
  };

  const removeHeader = (id: string) => {
    updateHeaders(
      selectedResponse.headers.filter((header) => header.id !== id)
    );
  };

  return (
    <div className="space-y-4">
      {selectedResponse.headers.map((header) => (
        <div key={header.id} className="flex items-center space-x-2">
          <Input
            placeholder="Key"
            value={header.key}
            onChange={(e) => updateHeader(header.id, "key", e.target.value)}
          />
          <Input
            placeholder="Value"
            value={header.value}
            onChange={(e) => updateHeader(header.id, "value", e.target.value)}
          />
          <Button
            variant="ghost"
            size="icon"
            onClick={() => removeHeader(header.id)}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      ))}
      <Button onClick={addHeader}>
        <Plus className="mr-2 h-4 w-4" /> Add Header
      </Button>
    </div>
  );
}
