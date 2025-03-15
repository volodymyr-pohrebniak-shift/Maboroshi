import { useState } from "react";
import { useAtom } from "jotai";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { environmentsAtom, selectedEnvironmentIdAtom } from "../state/atoms";

export function EnvironmentSidebar() {
  const [environments] = useAtom(environmentsAtom);
  const [selectedEnvironmentId, setSelectedEnvironmentId] = useAtom(
    selectedEnvironmentIdAtom
  );
  const [isExpanded, setIsExpanded] = useState(true);

  const toggleSidebar = () => {
    setIsExpanded(!isExpanded);
  };

  return (
    <div
      className={`bg-background transition-all duration-300 ease-in-out ${
        isExpanded ? "w-48" : "w-12"
      } flex flex-col`}
    >
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
