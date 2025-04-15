import { useAtom } from "jotai";
import { Label } from "../../components/ui/label";
import { Textarea } from "../../components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../components/ui/select";
import {
  environmentsAtom,
  selectedEnvironmentIdAtom,
  selectedRouteIdAtom,
  selectedResponseIdAtom,
} from "../../state/atoms";
import { HTTP_STATUS_CODES } from "../../data";
import React from "react";
import { SelectGroup, SelectLabel, SelectSeparator } from "@radix-ui/react-select";
import { Clock } from "lucide-react";
import { Tooltip } from "../ui/tooltip";
import { Input } from "../ui/input";

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
        <div className="flex flex-row">
          <Select
            value={selectedResponse.statusCode.toString()}
            onValueChange={(value: any) => updateResponse({ statusCode: value })}
          >
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Select status code" />
            </SelectTrigger>
            <SelectContent className="p-4">
              {Object.entries(HTTP_STATUS_CODES).map(
                ([key, group], index, arr) => (
                  <React.Fragment key={key}>
                    <SelectGroup>
                      <SelectLabel className="pb-3 text-sm font-bold text-gray-600">
                        {group.label}
                      </SelectLabel>
                      {group.codes.map(({ code, name }) => (
                        <SelectItem key={code} value={code.toString()}>
                          <span className="text-base">{code} – {name}</span>
                        </SelectItem>
                      ))}
                    </SelectGroup>
                    {index < arr.length - 1 && (
                      <SelectSeparator className="pb-3" />
                    )}
                  </React.Fragment>
                )
              )}
            </SelectContent>
          </Select>
          <div className="flex flex-row justify-center items-center">
            <Tooltip content="Response delay (in milliseconds)">
              <Clock className="mx-4" />
            </Tooltip>
            <Input
              id="delay"
              type="number"
              value={selectedResponse.delay}
              onChange={(e: any) => updateResponse({ delay: e.target.value })}
            />
          </div>
        </div>
      </div>
      <div className="space-y-2">
        <Label htmlFor="response-body">Response Body</Label>
        <Textarea
          id="response-body"
          value={selectedResponse.body}
          onChange={(e: any) => updateResponse({ body: e.target.value })}
          placeholder="Enter JSON response"
          rows={10}
        />
      </div>
    </div>
  );
}
