import { useAtom } from "jotai";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ResponseConfig } from "@/components/routes-edit/route-response-config";
import { HeadersConfig } from "@/components/routes-edit/route-headers-config";
import { RulesEditor } from "@/components/routes-edit/route-rules-editor";
import { Shuffle, Repeat, Plus, Trash2 } from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
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

const HTTP_METHODS = ["GET", "POST", "PUT", "DELETE", "PATCH"];

export function RouteConfig() {
  const [environments, setEnvironments] = useAtom(environmentsAtom);
  const [selectedEnvironmentId] = useAtom(selectedEnvironmentIdAtom);
  const [selectedRouteId] = useAtom(selectedRouteIdAtom);
  const [selectedResponseId, setSelectedResponseId] = useAtom(
    selectedResponseIdAtom
  );

  const selectedEnvironment = environments.find(
    (env) => env.id === selectedEnvironmentId
  );
  const selectedRoute = selectedEnvironment?.routes.find(
    (route) => route.id === selectedRouteId
  );

  if (!selectedRoute) {
    return <div className="p-6">Select a route to configure</div>;
  }

  const updateRoute = (updates: Partial<typeof selectedRoute>) => {
    setEnvironments(
      environments.map((env) =>
        env.id === selectedEnvironmentId
          ? {
              ...env,
              routes: env.routes.map((route) =>
                route.id === selectedRouteId ? { ...route, ...updates } : route
              ),
            }
          : env
      )
    );
  };

  const toggleMethod = (method: string) => {
    const updatedMethods = selectedRoute.methods.includes(method)
      ? selectedRoute.methods.filter((m) => m !== method)
      : [...selectedRoute.methods, method];
    updateRoute({ methods: updatedMethods });
  };

  const addResponse = () => {
    const newResponse = {
      id: Date.now().toString(),
      name: `Response ${selectedRoute.responses.length + 1}`,
      statusCode: "200",
      body: "{}",
      headers: [],
      rules: [],
    };
    updateRoute({ responses: [...selectedRoute.responses, newResponse] });
    setSelectedResponseId(newResponse.id);
  };

  const deleteResponse = () => {
    if (selectedResponseId) {
      const updatedResponses = selectedRoute.responses.filter(
        (r) => r.id !== selectedResponseId
      );
      updateRoute({ responses: updatedResponses });
      setSelectedResponseId(updatedResponses[0]?.id || null);
    }
  };

  return (
    <div className="p-6 space-y-6">
      <div className="space-y-2">
        <Label htmlFor="route-path">Route Path</Label>
        <Input
          id="route-path"
          value={selectedRoute.path}
          onChange={(e) => updateRoute({ path: e.target.value })}
          placeholder="/api/your-route"
        />
      </div>
      <div className="space-y-2">
        <Label>HTTP Methods</Label>
        <div className="flex flex-wrap gap-2">
          {HTTP_METHODS.map((method) => (
            <Button
              key={method}
              variant={
                selectedRoute.methods.includes(method) ? "default" : "outline"
              }
              onClick={() => toggleMethod(method)}
            >
              {method}
            </Button>
          ))}
        </div>
      </div>
      <div className="flex items-center space-x-2">
        <Button variant="outline" size="icon" onClick={addResponse}>
          <Plus className="h-4 w-4" />
        </Button>
        <Select
          value={selectedResponseId || ""}
          onValueChange={setSelectedResponseId}
        >
          <SelectTrigger className="w-[200px]">
            <SelectValue>
              {selectedRoute.responses.find((r) => r.id === selectedResponseId)
                ?.name || "Select Response"}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            {selectedRoute.responses.map((response) => (
              <SelectItem key={response.id} value={response.id}>
                {response.name} ({response.statusCode})
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Button variant="outline" size="icon" title="Random Response">
          <Shuffle className="h-4 w-4" />
        </Button>
        <Button variant="outline" size="icon" title="Sequential Response">
          <Repeat className="h-4 w-4" />
        </Button>
      </div>
      <div className="flex justify-between items-baseline">
        <Tabs defaultValue="response">
          <TabsList>
            <TabsTrigger value="response">Response</TabsTrigger>
            <TabsTrigger value="headers">Headers</TabsTrigger>
            <TabsTrigger value="rules">Rules</TabsTrigger>
          </TabsList>
          <TabsContent value="response">
            <ResponseConfig />
          </TabsContent>
          <TabsContent value="headers">
            <HeadersConfig />
          </TabsContent>
          <TabsContent value="rules">
            <RulesEditor />
          </TabsContent>
        </Tabs>
        <Button
          variant="outline"
          size="icon"
          onClick={deleteResponse}
          disabled={!selectedResponseId}
        >
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
