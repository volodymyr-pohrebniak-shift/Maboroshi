import { Button } from "@/components/ui/button";
import { useAtom } from "jotai";
import { Upload, SaveIcon } from "lucide-react";
import { environmentsAtom } from "../state/atoms";
import { updateEnvironments } from "../services/api-service";
import { useToast } from "./ui/use-toast";
import { useState } from "react";

export function Header() {
const [ environments,] = useAtom(environmentsAtom);
const { toast } = useToast();
const [isSaving, setIsSaving] = useState(false);

const handleSaveClick = async () => {
  setIsSaving(true)

    try {
      const result = await updateEnvironments(environments);

      // Show success toast
      toast({
        title: "Success",
        description: result.message || "Configuration saved successfully!",
        duration: 2000, // Auto dismiss after 2 seconds
      })
    } catch (error: any) {
      // Show error toast that requires user interaction
      toast({
        title: "Error",
        description: error.message || "Failed to save configuration",
        variant: "destructive",
        duration: Number.POSITIVE_INFINITY, // Won't auto-dismiss
      })
    } finally {
      setIsSaving(false)
    }
};

  return (
    <header className="flex items-center justify-between px-6 py-4 border-b">
      <h1 className="text-2xl font-bold">MABOROSHI</h1>
      <div className="flex space-x-2">
        <Button variant="outline">
          <Upload className="mr-2 h-4 w-4" />
          Import OpenAPI
        </Button>
        <Button onClick={handleSaveClick}>
          <SaveIcon className="mr-2 h-4 w-4" />
          Save Configuration
        </Button>
      </div>
    </header>
  );
}
