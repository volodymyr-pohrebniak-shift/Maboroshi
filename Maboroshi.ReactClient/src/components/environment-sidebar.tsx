import { useState } from "react";
import { useAtom } from "jotai";
import { Button } from "../components/ui/button";
import { ScrollArea } from "../components/ui/scroll-area";
import { v4 as uuidv4 } from "uuid";
import { ChevronLeft, ChevronRight, Circle, CircleCheck, PlusCircle } from "lucide-react";
import { Tooltip } from "./ui/tooltip";
import {
  Environment,
  environmentsAtom,
  selectedEnvironmentIdAtom,
} from "../state/atoms";
import { generateRandomName } from "../lib/utils";

export function EnvironmentSidebar() {
  const [environments, setEnvironments] = useAtom(environmentsAtom);
  const [selectedEnvironmentId, setSelectedEnvironmentId] = useAtom(
    selectedEnvironmentIdAtom
  );
  const [isExpanded, setIsExpanded] = useState(true);

  const toggleSidebar = () => {
    setIsExpanded(!isExpanded);
  };

  const addNewEnvironment = () => {
    const newEnvironment: Environment = {
      id: uuidv4(),
      name: generateRandomName(),
      routes: [],
      active: false
    };
    setEnvironments([...environments, newEnvironment]);
  };

  const makeActive = (id: string) => {
    setEnvironments(environments.map((env) => ({ ...env, active: env.id === id })));
  };

  // the server serves the first environment when none is marked active
  const activeEnvironmentId = environments.find((env) => env.active)?.id ?? environments[0]?.id;

  return (
    <div
      className={`border-r bg-background transition-all duration-300 ease-in-out ${
        isExpanded ? "w-60" : "w-12"
      } flex flex-col`}
    >
      <div
        className={`transition-all duration-300 ease-in-out flex items-center justify-between ${
          isExpanded ? "pl-4" : "pl-0"
        }`}
      >
        <Button
          className={`transition-all duration-300 ease-in-out flex items-center justify-between ${
            isExpanded ? "w-full" : "w-0 p-0"
          }`}
          onClick={addNewEnvironment}
        >
          <PlusCircle className="mr-2 h-4 w-4" />
          New Environment
        </Button>
        <Button
          variant="ghost"
          size="icon"
          className="self-end m-2"
          onClick={toggleSidebar}
        >
          {isExpanded ? (
            <ChevronLeft className="h-4 w-4" />
          ) : (
            <ChevronRight className="h-4 w-4" />
          )}
        </Button>
      </div>

      {isExpanded && (
        <ScrollArea className="flex-grow">
          <div className="p-4 space-y-2">
            {environments.map((env) => (
              <div key={env.id} className="flex items-center space-x-1">
                <Button
                  variant={selectedEnvironmentId === env.id ? "default" : "ghost"}
                  className="flex-1 justify-start"
                  onClick={() => setSelectedEnvironmentId(env.id)}
                >
                  {env.name}
                </Button>
                <Tooltip
                  content={
                    env.id === activeEnvironmentId
                      ? "Active environment"
                      : "Make active (applies after saving)"
                  }
                >
                  <Button variant="ghost" size="icon" onClick={() => makeActive(env.id)}>
                    {env.id === activeEnvironmentId ? (
                      <CircleCheck className="h-4 w-4 text-primary" />
                    ) : (
                      <Circle className="h-4 w-4 text-muted-foreground" />
                    )}
                  </Button>
                </Tooltip>
              </div>
            ))}
          </div>
        </ScrollArea>
      )}
    </div>
  );
}
