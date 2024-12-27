import { Button } from "@/components/ui/button";
import { PlusCircle, Upload } from "lucide-react";

export function Header() {
  return (
    <header className="flex items-center justify-between px-6 py-4 border-b">
      <h1 className="text-2xl font-bold">MABOROSHI</h1>
      <div className="flex space-x-2">
        <Button variant="outline">
          <Upload className="mr-2 h-4 w-4" />
          Import OpenAPI
        </Button>
        <Button>
          <PlusCircle className="mr-2 h-4 w-4" />
          New Environment
        </Button>
      </div>
    </header>
  );
}
