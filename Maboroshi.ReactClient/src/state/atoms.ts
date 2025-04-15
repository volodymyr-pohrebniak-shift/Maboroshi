import { atom } from "jotai";

export type RuleCondition = "AND" | "OR";

export type BaseRule = {
  type: string;
  id: string;
};

export type SimpleRule = BaseRule & {
  type: "Header" | "Query" | "Cookie " | "Body" | "Route";
  key: string;
  operation: string;
  value: string;
  negate: boolean;
};

export type RuleGroup = BaseRule & {
  type: "Aggregate";
  op: RuleCondition;
  rules: Rule[];
};

export type Rule = SimpleRule | RuleGroup;

export type Header = {
  id: string;
  key: string;
  value: string;
};

export type Response = {
  id: string;
  name: string;
  statusCode: number;
  body: string;
  headers: Header[];
  rules: Rule[];
  description?: string;
  delay: number;
  strictTemplateErrors: boolean;
  disableTemplating: boolean;
};

export type Route = {
  id: string;
  urlTemplate: string;
  httpMethod: string[];
  responses: Response[];
  enabled: boolean;
  responseSelectionStrategy: ResponseSelectionStrategy;
};

export type ResponseSelectionStrategy = "Default" | "Random" | "Sequence";

export type Environment = {
  id: string;
  name: string;
  routes: Route[];
  active: boolean;
};

const initialState: Environment[] = [];

export const environmentsAtom = atom<Environment[]>(initialState);

export const selectedEnvironmentIdAtom = atom<string | null>(null);

export const selectedRouteIdAtom = atom<string | null>(null);

export const selectedResponseIdAtom = atom<string | null>(null);
