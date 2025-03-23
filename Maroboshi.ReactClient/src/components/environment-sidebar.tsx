import { useState } from "react";
import { useAtom } from "jotai";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { v4 as uuidv4 } from "uuid";
import { ChevronLeft, ChevronRight, PlusCircle } from "lucide-react";
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
      routes: []
    };
    setEnvironments([...environments, newEnvironment]);
  };

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
              <Button
                key={env.id}
                variant={selectedEnvironmentId === env.id ? "default" : "ghost"}
                className="w-full justify-start"
                onClick={() => setSelectedEnvironmentId(env.id)}
              >
                {env.name}
              </Button>
            ))}
          </div>
        </ScrollArea>
      )}
    </div>
  );
}
