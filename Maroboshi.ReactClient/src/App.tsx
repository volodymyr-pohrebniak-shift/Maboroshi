import { useEffect } from "react";
import { useAtom } from "jotai";
import { ThemeProvider } from "./components/theme-provider";
import { Header } from "./components/header";
import { EnvironmentSidebar } from "./components/environment-sidebar";
import { RouteSidebar } from "./components/route-sidebar";
import { RouteConfig } from "./components/routes-edit/route-config";
import { environmentsAtom } from "./state/atoms";
import { fetchInitialState } from "./services/api-service";
import { Toaster } from "./components/ui/toaster";

function App() {
  const [, setEnvironments] = useAtom(environmentsAtom);

  useEffect(() => {
    fetchInitialState().then(setEnvironments);
  }, [setEnvironments]);

  return (
    <ThemeProvider defaultTheme="system" storageKey="vite-ui-theme">
      <div className="flex flex-col h-screen">
        <Header />
        <div className="flex flex-1 overflow-hidden">
          <EnvironmentSidebar />
          <RouteSidebar />
          <main className="flex-1 overflow-y-auto">
            <RouteConfig />
          </main>
        </div>
      </div>
      <Toaster />
    </ThemeProvider>
  );
}

export default App;
