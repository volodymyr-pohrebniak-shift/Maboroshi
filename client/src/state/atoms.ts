import { atom } from 'jotai'

export type Rule = {
  id: string
  type: string
  key: string
  operation: string
  value: string
  negate: boolean
}

export type Header = {
  id: string
  key: string
  value: string
}

export type Response = {
  id: string
  name: string
  statusCode: string
  body: string
  headers: Header[]
  rules: Rule[]
}

export type Route = {
  id: string
  path: string
  methods: string[]
  responses: Response[]
  enabled: boolean
}

export type Environment = {
  id: string
  name: string
  routes: Route[]
}

const initialState: Environment[] = []

export const environmentsAtom = atom<Environment[]>(initialState)

export const selectedEnvironmentIdAtom = atom<string | null>(null)

export const selectedRouteIdAtom = atom<string | null>(null)

export const selectedResponseIdAtom = atom<string | null>(null)

