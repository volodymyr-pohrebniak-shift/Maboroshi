import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";
import {
  uniqueNamesGenerator,
  Config,
  adjectives,
  animals,
} from "unique-names-generator";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

const customConfig: Config = {
  dictionaries: [adjectives, animals],
  separator: "_",
  length: 2,
};

export const generateRandomName = () => {
  return uniqueNamesGenerator(customConfig);
};
