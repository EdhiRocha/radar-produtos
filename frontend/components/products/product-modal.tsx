"use client";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Product } from "@/lib/types";
import Image from "next/image";
import { ScoreBadge } from "./score-badge";
import { SentimentBadge } from "./sentiment-badge";
import { CompetitionBadge } from "./competition-badge";
import {
  Star,
  Package,
  TrendingUp,
  MessageSquare,
  ExternalLink,
  ShoppingCart,
  DollarSign,
  BarChart3,
  TrendingDown,
  Award,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
  LineChart,
  Line,
} from "recharts";

interface ProductModalProps {
  product: Product | null;
  open: boolean;
  onClose: () => void;
}

export function ProductModal({ product, open, onClose }: ProductModalProps) {
  if (!product) return null;

  const lucro = product.estimatedSalePrice - product.supplierPrice;
  const roi = ((lucro / product.supplierPrice) * 100).toFixed(1);

  const sentimentData = [
    { name: "Positivo", value: 70, color: "#22c55e" },
    { name: "Neutro", value: 20, color: "#eab308" },
    { name: "Negativo", value: 10, color: "#ef4444" },
  ];

  const profitBreakdown = [
    { name: "Custo", valor: product.supplierPrice, fill: "#ef4444" },
    { name: "Lucro", valor: lucro, fill: "#22c55e" },
  ];

  const performanceMetrics = [
    {
      label: "Potencial de Lucro",
      value: lucro,
      formatted: `R$ ${lucro.toFixed(2)}`,
      color: "text-green-600",
      icon: DollarSign,
      progress: (lucro / product.estimatedSalePrice) * 100,
    },
    {
      label: "ROI Estimado",
      value: parseFloat(roi),
      formatted: `${roi}%`,
      color: "text-blue-600",
      icon: TrendingUp,
      progress: Math.min(parseFloat(roi), 100),
    },
    {
      label: "Volume de Vendas",
      value: product.orders,
      formatted: `${product.orders} pedidos`,
      color: "text-purple-600",
      icon: ShoppingCart,
      progress: Math.min((product.orders / 5000) * 100, 100),
    },
  ];

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-[95vw] sm:max-w-[90vw] lg:max-w-7xl max-h-[95vh] overflow-y-auto p-4 sm:p-6">
        {/* Header */}
        <DialogHeader className="border-b pb-4">
          <div className="flex flex-col sm:flex-row items-start justify-between gap-4">
            <div className="flex-1">
              <DialogTitle className="text-xl sm:text-2xl lg:text-3xl font-bold mb-2">
                {product.name}
              </DialogTitle>
              <div className="flex items-center gap-2 flex-wrap">
                <Badge variant="outline" className="gap-1">
                  <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                  <span className="font-bold">{product.rating}</span>
                </Badge>
                <Badge variant="outline">
                  {product.orders.toLocaleString()} vendas
                </Badge>
                <Badge variant="outline">{product.supplier}</Badge>
              </div>
            </div>
            <div className="flex flex-col items-center">
              <span className="text-xs text-gray-500 mb-1">Score</span>
              <ScoreBadge
                score={product.score}
                className="text-xl sm:text-2xl px-3 sm:px-4 py-2"
              />
            </div>
          </div>
        </DialogHeader>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6">
          {/* Coluna Esquerda */}
          <div className="space-y-6">
            {/* Imagem */}
            <div className="relative aspect-square w-full overflow-hidden rounded-xl border-2 border-gray-200 bg-white shadow-lg">
              <Image
                src={product.imageUrl || "/placeholder.svg"}
                alt={product.name}
                fill
                className="object-cover"
              />
            </div>

            {/* Info Básica */}
            <Card className="shadow-lg">
              <CardHeader className="pb-4">
                <CardTitle className="text-lg flex items-center gap-2">
                  <Package className="h-5 w-5" />
                  Informações do Produto
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4 text-sm sm:text-base">
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Fornecedor:</span>
                  <Badge variant="secondary">{product.supplier}</Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Avaliação:</span>
                  <div className="flex items-center gap-1">
                    <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                    <span className="font-bold">{product.rating}</span>
                    <span className="text-gray-500">/5.0</span>
                  </div>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Vendas:</span>
                  <span className="font-bold text-blue-600">
                    {product.orders.toLocaleString()}
                  </span>
                </div>

                {/* Novas métricas */}
                {product.shippingDays && (
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600">Prazo de Entrega:</span>
                    <Badge
                      variant={
                        product.shippingDays <= 15
                          ? "default"
                          : product.shippingDays <= 30
                          ? "secondary"
                          : "destructive"
                      }
                    >
                      {product.shippingDays} dias
                    </Badge>
                  </div>
                )}

                {product.commissionRate && (
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600">Comissão:</span>
                    <span className="font-bold text-purple-600">
                      {product.commissionRate.toFixed(1)}%
                    </span>
                  </div>
                )}

                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Recursos:</span>
                  <div className="flex gap-2">
                    {product.hasVideo && (
                      <Badge variant="outline" className="bg-blue-50">
                        📹 Vídeo
                      </Badge>
                    )}
                    {product.hasPromotion && (
                      <Badge variant="outline" className="bg-orange-50">
                        🎁 Promoção
                      </Badge>
                    )}
                    {!product.hasVideo && !product.hasPromotion && (
                      <span className="text-sm text-muted-foreground">
                        Nenhum recurso extra
                      </span>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Loja do Fornecedor */}
            {product.shopName && product.shopUrl && (
              <Card className="shadow-lg border-blue-200 bg-blue-50/50">
                <CardHeader className="pb-3">
                  <CardTitle className="text-lg flex items-center gap-2 text-blue-700">
                    <Award className="h-5 w-5" />
                    Loja Oficial
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="bg-white rounded-lg p-3 border border-blue-200">
                    <div className="text-xs text-gray-500 mb-1">
                      Nome da Loja
                    </div>
                    <div className="font-bold text-blue-900">
                      {product.shopName}
                    </div>
                  </div>
                  <Button
                    className="w-full gap-2 bg-blue-600 hover:bg-blue-700"
                    onClick={() => window.open(product.shopUrl, "_blank")}
                  >
                    <ExternalLink className="h-4 w-4" />
                    Visitar Loja do Fornecedor
                  </Button>
                </CardContent>
              </Card>
            )}

            <Button
              className="w-full gap-2"
              size="lg"
              onClick={() => {
                if (product.supplierUrl) {
                  window.open(product.supplierUrl, "_blank");
                } else {
                  // Busca genérica no AliExpress com o nome do produto
                  const searchQuery = encodeURIComponent(
                    product.name.substring(0, 100)
                  );
                  window.open(
                    `https://www.aliexpress.com/wholesale?SearchText=${searchQuery}`,
                    "_blank"
                  );
                }
              }}
            >
              <ExternalLink className="h-4 w-4" />
              {product.supplierUrl
                ? "Ver Produto no AliExpress"
                : "Buscar no AliExpress"}
            </Button>
          </div>

          {/* Coluna Direita */}
          <div className="space-y-6">
            {/* Análise Financeira */}
            <Card className="border-green-200 bg-green-50/50 shadow-lg">
              <CardHeader className="pb-4">
                <CardTitle className="text-lg flex items-center gap-2 text-green-700">
                  <DollarSign className="h-5 w-5" />
                  Análise Financeira
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-5">
                <div className="grid grid-cols-2 gap-3">
                  <div className="bg-white rounded-lg p-3 border border-red-200">
                    <div className="text-xs text-gray-500">Custo</div>
                    <div className="text-lg font-bold text-red-600">
                      R$ {product.supplierPrice.toFixed(2)}
                    </div>
                  </div>
                  <div className="bg-white rounded-lg p-3 border border-green-200">
                    <div className="text-xs text-gray-500">Venda</div>
                    <div className="text-lg font-bold text-green-600">
                      R$ {product.estimatedSalePrice.toFixed(2)}
                    </div>
                  </div>
                </div>

                <div className="bg-white rounded-lg p-4 border border-green-300">
                  <div className="flex justify-between items-center mb-2">
                    <span className="text-sm text-gray-600">Lucro Bruto:</span>
                    <span className="text-xl font-bold text-green-600">
                      R$ {lucro.toFixed(2)}
                    </span>
                  </div>
                  <div className="flex justify-between items-center mb-3">
                    <span className="text-sm text-gray-600">Margem:</span>
                    <span className="text-xl sm:text-2xl font-bold text-green-600">
                      {product.marginPercent.toFixed(1)}%
                    </span>
                  </div>
                  <Progress value={product.marginPercent} className="h-3" />
                </div>

                <ResponsiveContainer width="100%" height={120}>
                  <BarChart data={profitBreakdown} layout="vertical">
                    <XAxis type="number" />
                    <YAxis type="category" dataKey="name" width={50} />
                    <Tooltip
                      formatter={(value) => `R$ ${Number(value).toFixed(2)}`}
                    />
                    <Bar dataKey="valor" radius={[0, 6, 6, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            {/* Concorrência */}
            <Card className="shadow-lg">
              <CardHeader className="pb-4">
                <CardTitle className="text-lg flex items-center gap-2">
                  <BarChart3 className="h-5 w-5" />
                  Concorrência
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Nível:</span>
                  <CompetitionBadge
                    level={product.competitionLevel.toLowerCase() as any}
                  />
                </div>
                <div className="bg-gray-50 rounded-lg p-3 text-sm text-center">
                  {product.competitionLevel.toLowerCase() === "baixa" && (
                    <span className="text-green-700">
                      <strong>Excelente!</strong> Poucos concorrentes.
                    </span>
                  )}
                  {product.competitionLevel.toLowerCase() === "media" && (
                    <span className="text-yellow-700">
                      <strong>Moderado.</strong> Foque em diferenciação.
                    </span>
                  )}
                  {product.competitionLevel.toLowerCase() === "alta" && (
                    <span className="text-red-700">
                      <strong>Alto.</strong> Mercado saturado.
                    </span>
                  )}
                </div>
              </CardContent>
            </Card>

            {/* Sentimento */}
            <Card className="shadow-lg">
              <CardHeader className="pb-4">
                <CardTitle className="text-lg flex items-center gap-2">
                  <MessageSquare className="h-5 w-5" />
                  Sentimento
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Análise:</span>
                  <SentimentBadge
                    sentiment={product.sentiment.toLowerCase() as any}
                  />
                </div>

                <ResponsiveContainer width="100%" height={160}>
                  <PieChart>
                    <Pie
                      data={sentimentData}
                      cx="50%"
                      cy="50%"
                      innerRadius={40}
                      outerRadius={65}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {sentimentData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>

                <div className="grid grid-cols-3 gap-2 text-xs">
                  {sentimentData.map((item, index) => (
                    <div key={index} className="text-center">
                      <div
                        className="w-3 h-3 rounded-full mx-auto mb-1"
                        style={{ backgroundColor: item.color }}
                      />
                      <div className="font-medium">{item.name}</div>
                      <div className="font-bold">{item.value}%</div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
