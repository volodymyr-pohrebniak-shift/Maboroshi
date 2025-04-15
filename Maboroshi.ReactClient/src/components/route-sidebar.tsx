import { useAtom } from "jotai";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { ScrollArea } from "../components/ui/scroll-area";
import { MoreHorizontal, PlusCircle, Search } from "lucide-react";
import { v4 as uuidv4 } from 'uuid';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "../components/ui/dropdown-menu";
import {
  environmentsAtom,
  Route,
  selectedEnvironmentIdAtom,
  selectedResponseIdAtom,
  selectedRouteIdAtom,
} from "../state/atoms";

export function RouteSidebar() {
  const [environments, setEnvironments] = useAtom(environmentsAtom);
  const [selectedEnvironmentId] = useAtom(selectedEnvironmentIdAtom);
  const [selectedRouteId, setSelectedRouteId] = useAtom(selectedRouteIdAtom);
  const [, setSelectedResponseId] = useAtom(
      selectedResponseIdAtom
    );

  const selectedEnvironment = environments.find(
    (env) => env.id === selectedEnvironmentId
  );
  const routes = selectedEnvironment?.routes || [];

  const addNewRoute = () => {
    if (selectedEnvironmentId) {
      const newRoute: Route = {
        id: uuidv4(),
        urlTemplate: `/api/new-route-${routes.length + 1}`,
        httpMethod: ["GET"],
        responses: [
          {
            id: uuidv4(),
            body: '',
            headers: [],
            rules: [],
            statusCode: 200,
            name: 'Response 1',
            delay: 0,
            disableTemplating: false,
            strictTemplateErrors: false
          }
        ],
        enabled: true,
        responseSelectionStrategy: "Default"
      };
      setEnvironments(
        environments.map((env) =>
          env.id === selectedEnvironmentId
            ? { ...env, routes: [...env.routes, newRoute] }
            : env
        )
      );
    }
  };

  const toggleRoute = (routeId: string) => {
    setEnvironments(
      environments.map((env) =>
        env.id === selectedEnvironmentId
          ? {
              ...env,
              routes: env.routes.map((route) =>
                route.id === routeId
                  ? { ...route, enabled: !route.enabled }
                  : route
              ),
            }
          : env
      )
    );
  };

  const duplicateRoute = (routeId: string) => {
    const routeToDuplicate = routes.find((route) => route.id === routeId);
    if (routeToDuplicate) {
      const newRoute = {
        ...routeToDuplicate,
          id: Date.now().toString(),
          urlTemplate: `${routeToDuplicate.urlTemplate}-copy`,
      };
      setEnvironments(
        environments.map((env) =>
          env.id === selectedEnvironmentId
            ? { ...env, routes: [...env.routes, newRoute] }
            : env
        )
      );
    }
  };

  const deleteRoute = (routeId: string) => {
    setEnvironments(
      environments.map((env) =>
        env.id === selectedEnvironmentId
          ? {
              ...env,
              routes: env.routes.filter((route) => route.id !== routeId),
            }
          : env
      )
    );
  };

  return (
    <div className="w-64 border-r bg-background">
      <div className="p-4 space-y-4">
        <Button onClick={addNewRoute} className="w-full">
          <PlusCircle className="mr-2 h-4 w-4" /> New Route
        </Button>
        <div className="relative">
          <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input placeholder="Search routes" className="pl-8" />
        </div>
      </div>
      <ScrollArea className="h-[calc(100vh-9rem)]">
        <div className="p-4 space-y-2">
          {routes.map((route) => (
            <div key={route.id} className="flex items-center justify-between">
              <Button
                variant={selectedRouteId === route.id ? "default" : "ghost"}
                className={`w-full justify-start ${
                  route.enabled ? "" : "opacity-50"
                }`}
                onClick={() => {
                  setSelectedRouteId(route.id);
                  setSelectedResponseId(route.responses[0]?.id ?? null);
                }}
              >
                {route.urlTemplate}
              </Button>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="sm">
                    <MoreHorizontal className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem onClick={() => toggleRoute(route.id)}>
                    {route.enabled ? "Disable" : "Enable"}
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => duplicateRoute(route.id)}>
                    Duplicate
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => deleteRoute(route.id)}>
                    Delete
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          ))}
        </div>
      </ScrollArea>
    </div>
  );
}
