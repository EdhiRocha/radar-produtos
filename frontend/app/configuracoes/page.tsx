"use client";

import { useState } from "react";
import { Sidebar } from "@/components/layout/sidebar";
import { Header } from "@/components/layout/header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Settings } from 'lucide-react';
import { useToast } from "@/hooks/use-toast";

export default function ConfigPage() {
  const { toast } = useToast();
  const [config, setConfig] = useState({
    margemMin: "50",
    margemMax: "80",
    pesoVendas: "25",
    pesoConcorrencia: "25",
    pesoSentimento: "25",
    pesoMargem: "25",
  });

  const handleSave = () => {
    // Mock save - in real app would save to backend
    toast({
      title: "Configurações salvas",
      description: "Suas preferências foram atualizadas com sucesso!",
    });
  };

  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <Header />
      <main className="ml-64 pt-16">
        <div className="p-8 space-y-6">
          <div>
            <h2 className="text-3xl font-bold tracking-tight">Configurações</h2>
            <p className="text-muted-foreground">
              Configure suas preferências de análise
            </p>
          </div>

          <div className="max-w-2xl space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Settings className="h-5 w-5" />
                  Preferências de Margem
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="margemMin">Margem Mínima Desejada (%)</Label>
                  <Input
                    id="margemMin"
                    type="number"
                    value={config.margemMin}
                    onChange={(e) =>
                      setConfig({ ...config, margemMin: e.target.value })
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="margemMax">Margem Máxima (%)</Label>
                  <Input
                    id="margemMax"
                    type="number"
                    value={config.margemMax}
                    onChange={(e) =>
                      setConfig({ ...config, margemMax: e.target.value })
                    }
                  />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Pesos dos Critérios de Score</CardTitle>
                <p className="text-sm text-muted-foreground">
                  Ajuste a importância de cada critério na pontuação final (total
                  deve ser 100%)
                </p>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="pesoVendas">Peso: Vendas (%)</Label>
                  <Input
                    id="pesoVendas"
                    type="number"
                    value={config.pesoVendas}
                    onChange={(e) =>
                      setConfig({ ...config, pesoVendas: e.target.value })
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="pesoConcorrencia">
                    Peso: Concorrência (%)
                  </Label>
                  <Input
                    id="pesoConcorrencia"
                    type="number"
                    value={config.pesoConcorrencia}
                    onChange={(e) =>
                      setConfig({ ...config, pesoConcorrencia: e.target.value })
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="pesoSentimento">Peso: Sentimento (%)</Label>
                  <Input
                    id="pesoSentimento"
                    type="number"
                    value={config.pesoSentimento}
                    onChange={(e) =>
                      setConfig({ ...config, pesoSentimento: e.target.value })
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="pesoMargem">Peso: Margem (%)</Label>
                  <Input
                    id="pesoMargem"
                    type="number"
                    value={config.pesoMargem}
                    onChange={(e) =>
                      setConfig({ ...config, pesoMargem: e.target.value })
                    }
                  />
                </div>
                <div className="pt-2 border-t border-border">
                  <p className="text-sm">
                    <span className="text-muted-foreground">Total: </span>
                    <span className="font-bold">
                      {parseInt(config.pesoVendas) +
                        parseInt(config.pesoConcorrencia) +
                        parseInt(config.pesoSentimento) +
                        parseInt(config.pesoMargem)}
                      %
                    </span>
                  </p>
                </div>
              </CardContent>
            </Card>

            <Button onClick={handleSave} className="w-full">
              Salvar Configurações
            </Button>
          </div>
        </div>
      </main>
    </div>
  );
}
