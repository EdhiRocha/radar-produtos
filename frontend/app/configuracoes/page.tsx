"use client";

import { useState, useEffect } from "react";
import { Sidebar } from "@/components/layout/sidebar";
import { Header } from "@/components/layout/header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  Settings,
  DollarSign,
  Package,
  TrendingUp,
  Filter,
} from "lucide-react";
import { toast } from "sonner";

interface MarketplaceConfig {
  id: number;
  minMarginPercent: number;
  targetMarginPercent: number;
  mercadoLivreFixedFee: number;
  mercadoLivrePercentFee: number;
  mercadoLivreBoostFee: number;
  importTaxPercent: number;
  companyTaxPercent: number;
  usdToBrlRate: number;
  autoUpdateExchangeRate: boolean;
  defaultShippingCostUsd: number;
  useEstimatedShipping: boolean;
  minSalesVolume: number;
  minSupplierRating: number;
  maxDeliveryDays: number;
  weightMargin: number;
  weightSales: number;
  weightRating: number;
  weightDelivery: number;
}

export default function ConfigPage() {
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [config, setConfig] = useState<MarketplaceConfig>({
    id: 1,
    minMarginPercent: 30,
    targetMarginPercent: 50,
    mercadoLivreFixedFee: 20,
    mercadoLivrePercentFee: 15,
    mercadoLivreBoostFee: 5,
    importTaxPercent: 60,
    companyTaxPercent: 8.93,
    usdToBrlRate: 5.7,
    autoUpdateExchangeRate: true,
    defaultShippingCostUsd: 10,
    useEstimatedShipping: false,
    minSalesVolume: 100,
    minSupplierRating: 4.0,
    maxDeliveryDays: 30,
    weightMargin: 0.4,
    weightSales: 0.3,
    weightRating: 0.2,
    weightDelivery: 0.1,
  });

  useEffect(() => {
    loadConfig();
  }, []);

  const loadConfig = async () => {
    try {
      setIsLoading(true);
      const response = await fetch(
        "http://localhost:5001/api/marketplaceconfig"
      );
      if (response.ok) {
        const data = await response.json();
        setConfig(data);
      }
    } catch (error) {
      console.error("Erro ao carregar configurações:", error);
      toast.error("Erro ao carregar configurações");
    } finally {
      setIsLoading(false);
    }
  };

  const handleSave = async () => {
    try {
      setIsSaving(true);
      const response = await fetch(
        "http://localhost:5001/api/marketplaceconfig",
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(config),
        }
      );

      if (response.ok) {
        toast.success("Configurações salvas com sucesso!");
      } else {
        throw new Error("Erro ao salvar");
      }
    } catch (error) {
      console.error("Erro ao salvar configurações:", error);
      toast.error("Erro ao salvar configurações");
    } finally {
      setIsSaving(false);
    }
  };

  const totalWeights =
    config.weightMargin +
    config.weightSales +
    config.weightRating +
    config.weightDelivery;

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background">
        <Sidebar />
        <Header />
        <main className="ml-64 pt-16">
          <div className="p-8 flex items-center justify-center">
            <div className="text-center space-y-3">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto" />
              <p className="text-muted-foreground">
                Carregando configurações...
              </p>
            </div>
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <Header />
      <main className="ml-64 pt-16">
        <div className="p-8 space-y-6">
          <div>
            <h2 className="text-3xl font-bold tracking-tight">
              Configurações de Marketplace
            </h2>
            <p className="text-muted-foreground">
              Configure os parâmetros para cálculo de viabilidade de produtos
            </p>
          </div>

          <div className="max-w-4xl space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <TrendingUp className="h-5 w-5" />
                  Margens de Lucro
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="minMargin">
                      Margem Mínima Aceitável (%)
                    </Label>
                    <Input
                      id="minMargin"
                      type="number"
                      step="0.1"
                      value={config.minMarginPercent}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          minMarginPercent: parseFloat(e.target.value),
                        })
                      }
                    />
                    <p className="text-xs text-muted-foreground">
                      Produtos abaixo desta margem serão marcados como inviáveis
                    </p>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="targetMargin">Margem Alvo (%)</Label>
                    <Input
                      id="targetMargin"
                      type="number"
                      step="0.1"
                      value={config.targetMarginPercent}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          targetMarginPercent: parseFloat(e.target.value),
                        })
                      }
                    />
                    <p className="text-xs text-muted-foreground">
                      Margem desejada para cálculo do preço de venda sugerido
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <DollarSign className="h-5 w-5" />
                  Taxas e Impostos
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-4">
                  <div>
                    <h4 className="text-sm font-medium mb-3">Mercado Livre</h4>
                    <div className="grid grid-cols-3 gap-4">
                      <div className="space-y-2">
                        <Label htmlFor="mlFixedFee">Taxa Fixa (R$)</Label>
                        <Input
                          id="mlFixedFee"
                          type="number"
                          step="0.01"
                          value={config.mercadoLivreFixedFee}
                          onChange={(e) =>
                            setConfig({
                              ...config,
                              mercadoLivreFixedFee: parseFloat(e.target.value),
                            })
                          }
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="mlPercentFee">Taxa Variável (%)</Label>
                        <Input
                          id="mlPercentFee"
                          type="number"
                          step="0.1"
                          value={config.mercadoLivrePercentFee}
                          onChange={(e) =>
                            setConfig({
                              ...config,
                              mercadoLivrePercentFee: parseFloat(
                                e.target.value
                              ),
                            })
                          }
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="mlBoostFee">Taxa Boost (%)</Label>
                        <Input
                          id="mlBoostFee"
                          type="number"
                          step="0.1"
                          value={config.mercadoLivreBoostFee}
                          onChange={(e) =>
                            setConfig({
                              ...config,
                              mercadoLivreBoostFee: parseFloat(e.target.value),
                            })
                          }
                        />
                      </div>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="importTax">
                        Imposto de Importação (%)
                      </Label>
                      <Input
                        id="importTax"
                        type="number"
                        step="0.1"
                        value={config.importTaxPercent}
                        onChange={(e) =>
                          setConfig({
                            ...config,
                            importTaxPercent: parseFloat(e.target.value),
                          })
                        }
                      />
                      <p className="text-xs text-muted-foreground">
                        Aplicado sobre produto + frete
                      </p>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="companyTax">Impostos PJ (%)</Label>
                      <Input
                        id="companyTax"
                        type="number"
                        step="0.01"
                        value={config.companyTaxPercent}
                        onChange={(e) =>
                          setConfig({
                            ...config,
                            companyTaxPercent: parseFloat(e.target.value),
                          })
                        }
                      />
                      <p className="text-xs text-muted-foreground">
                        Simples Nacional ou outro regime tributário
                      </p>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Package className="h-5 w-5" />
                  Câmbio e Frete
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="usdRate">Taxa USD → BRL</Label>
                    <Input
                      id="usdRate"
                      type="number"
                      step="0.01"
                      value={config.usdToBrlRate}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          usdToBrlRate: parseFloat(e.target.value),
                        })
                      }
                    />
                  </div>
                  <div className="space-y-2">
                    <Label
                      htmlFor="autoUpdate"
                      className="flex items-center gap-2"
                    >
                      Atualizar automaticamente
                    </Label>
                    <div className="flex items-center gap-2 h-10">
                      <Switch
                        id="autoUpdate"
                        checked={config.autoUpdateExchangeRate}
                        onCheckedChange={(checked) =>
                          setConfig({
                            ...config,
                            autoUpdateExchangeRate: checked,
                          })
                        }
                      />
                      <span className="text-sm text-muted-foreground">
                        {config.autoUpdateExchangeRate ? "Ativo" : "Inativo"}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="defaultShipping">Frete Padrão (USD)</Label>
                    <Input
                      id="defaultShipping"
                      type="number"
                      step="0.01"
                      value={config.defaultShippingCostUsd}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          defaultShippingCostUsd: parseFloat(e.target.value),
                        })
                      }
                    />
                  </div>
                  <div className="space-y-2">
                    <Label
                      htmlFor="useEstimated"
                      className="flex items-center gap-2"
                    >
                      Usar estimativa por categoria
                    </Label>
                    <div className="flex items-center gap-2 h-10">
                      <Switch
                        id="useEstimated"
                        checked={config.useEstimatedShipping}
                        onCheckedChange={(checked) =>
                          setConfig({
                            ...config,
                            useEstimatedShipping: checked,
                          })
                        }
                      />
                      <span className="text-sm text-muted-foreground">
                        {config.useEstimatedShipping ? "Ativo" : "Inativo"}
                      </span>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Filter className="h-5 w-5" />
                  Filtros de Produtos
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="minSales">Volume Mínimo de Vendas</Label>
                    <Input
                      id="minSales"
                      type="number"
                      value={config.minSalesVolume}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          minSalesVolume: parseInt(e.target.value),
                        })
                      }
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="minRating">Rating Mínimo</Label>
                    <Input
                      id="minRating"
                      type="number"
                      step="0.1"
                      min="0"
                      max="5"
                      value={config.minSupplierRating}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          minSupplierRating: parseFloat(e.target.value),
                        })
                      }
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="maxDelivery">
                      Máx. Prazo de Entrega (dias)
                    </Label>
                    <Input
                      id="maxDelivery"
                      type="number"
                      value={config.maxDeliveryDays}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          maxDeliveryDays: parseInt(e.target.value),
                        })
                      }
                    />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Settings className="h-5 w-5" />
                  Pesos do Score de Viabilidade
                </CardTitle>
                <p className="text-sm text-muted-foreground mt-2">
                  Ajuste a importância de cada critério (total deve ser 1.0)
                </p>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="weightMargin">Peso: Margem</Label>
                    <Input
                      id="weightMargin"
                      type="number"
                      step="0.01"
                      min="0"
                      max="1"
                      value={config.weightMargin}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          weightMargin: parseFloat(e.target.value),
                        })
                      }
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="weightSales">Peso: Volume de Vendas</Label>
                    <Input
                      id="weightSales"
                      type="number"
                      step="0.01"
                      min="0"
                      max="1"
                      value={config.weightSales}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          weightSales: parseFloat(e.target.value),
                        })
                      }
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="weightRating">
                      Peso: Rating do Fornecedor
                    </Label>
                    <Input
                      id="weightRating"
                      type="number"
                      step="0.01"
                      min="0"
                      max="1"
                      value={config.weightRating}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          weightRating: parseFloat(e.target.value),
                        })
                      }
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="weightDelivery">
                      Peso: Prazo de Entrega
                    </Label>
                    <Input
                      id="weightDelivery"
                      type="number"
                      step="0.01"
                      min="0"
                      max="1"
                      value={config.weightDelivery}
                      onChange={(e) =>
                        setConfig({
                          ...config,
                          weightDelivery: parseFloat(e.target.value),
                        })
                      }
                    />
                  </div>
                </div>
                <div className="pt-2 border-t border-border">
                  <p className="text-sm">
                    <span className="text-muted-foreground">Total: </span>
                    <span
                      className={`font-bold ${
                        Math.abs(totalWeights - 1.0) < 0.01
                          ? "text-green-600"
                          : "text-red-600"
                      }`}
                    >
                      {totalWeights.toFixed(2)}
                    </span>
                    {Math.abs(totalWeights - 1.0) >= 0.01 && (
                      <span className="text-red-600 ml-2 text-xs">
                        (deve ser 1.0)
                      </span>
                    )}
                  </p>
                </div>
              </CardContent>
            </Card>

            <Button
              onClick={handleSave}
              className="w-full"
              disabled={isSaving || Math.abs(totalWeights - 1.0) >= 0.01}
            >
              {isSaving ? "Salvando..." : "Salvar Configurações"}
            </Button>
          </div>
        </div>
      </main>
    </div>
  );
}
