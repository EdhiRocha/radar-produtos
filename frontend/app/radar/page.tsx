"use client";

import { useState, useEffect } from "react";
import { Sidebar } from "@/components/layout/sidebar";
import { Header } from "@/components/layout/header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { MultiSelect, Option } from "@/components/ui/multi-select";
import {
  Search,
  ChevronLeft,
  ChevronRight,
  Star,
  ShoppingCart,
  Filter,
  X,
  Loader2,
  ChevronsLeft,
  ChevronsRight,
} from "lucide-react";
import { toast } from "sonner";
import Image from "next/image";
import { ProductModal } from "@/components/products/product-modal";
import { Product, Category } from "@/lib/types";
import { ScoreBadge } from "@/components/products/score-badge";
import { CompetitionBadge } from "@/components/products/competition-badge";
import { SentimentBadge } from "@/components/products/sentiment-badge";
import { analysisApi, categoryApi } from "@/lib/api";

export default function RadarPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [modalOpen, setModalOpen] = useState(false);

  // Categorias
  const [categories, setCategories] = useState<Option[]>([]);
  const [selectedCategories, setSelectedCategories] = useState<Option[]>([]);
  const [loadingCategories, setLoadingCategories] = useState(false);

  // Filtros da API AliExpress
  const [filters, setFilters] = useState({
    keyword: "",
    sort: "all",
    maxSalePrice: "",
    minSalePrice: "",
    minRating: "",
    minOrders: "",
  });

  // Paginação
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // Carregar categorias ao montar o componente
  useEffect(() => {
    loadCategories();
  }, []);

  const loadCategories = async () => {
    try {
      setLoadingCategories(true);
      const data = await categoryApi.getAll();
      const options: Option[] = data.map((cat) => ({
        value: cat.categoryId,
        label: cat.categoryName,
      }));
      setCategories(options);
    } catch (error) {
      console.error("Erro ao carregar categorias:", error);
      toast.error("Erro ao carregar categorias. Usando modo sem filtro.");
    } finally {
      setLoadingCategories(false);
    }
  };

  const searchProducts = async () => {
    try {
      setIsLoading(true);
      setProducts([]);

      const request: any = {
        pageNo: currentPage,
        pageSize: pageSize,
      };

      if (filters.keyword.trim()) request.keyword = filters.keyword.trim();

      // Concatenar IDs das categorias selecionadas
      if (selectedCategories.length > 0) {
        request.categoryIds = selectedCategories.map((c) => c.value).join(",");
      }

      if (filters.sort !== "all") request.sort = filters.sort;
      if (filters.maxSalePrice)
        request.maxSalePrice = parseFloat(filters.maxSalePrice);
      if (filters.minSalePrice)
        request.minSalePrice = parseFloat(filters.minSalePrice);

      const result = await analysisApi.run(request);

      let items = Array.isArray(result) ? result : result.products || [];

      if (filters.minRating) {
        items = items.filter(
          (p: Product) => p.rating >= parseFloat(filters.minRating)
        );
      }
      if (filters.minOrders) {
        items = items.filter(
          (p: Product) => p.orders >= parseInt(filters.minOrders)
        );
      }

      setProducts(items);

      if (items.length > 0) {
        toast.success(`${items.length} produtos encontrados!`);
      } else {
        toast.info("Nenhum produto encontrado. Tente outros filtros.");
      }
    } catch (error) {
      console.error("Erro ao buscar produtos:", error);
      toast.error(
        "Erro ao buscar produtos. Verifique os filtros e tente novamente."
      );
      setProducts([]);
    } finally {
      setIsLoading(false);
    }
  };

  const clearFilters = () => {
    setFilters({
      keyword: "",
      sort: "all",
      maxSalePrice: "",
      minSalePrice: "",
      minRating: "",
      minOrders: "",
    });
    setSelectedCategories([]);
    setCurrentPage(1);
    setProducts([]);
  };

  const handleProductClick = (product: Product) => {
    setSelectedProduct(product);
    setModalOpen(true);
  };

  const handlePageChange = (newPage: number) => {
    setCurrentPage(newPage);
  };

  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <Header />
      <main className="ml-64 pt-16">
        <div className="p-8 space-y-6">
          <div>
            <h2 className="text-3xl font-bold tracking-tight">
              Radar de Produtos
            </h2>
            <p className="text-muted-foreground">
              Busca avançada de produtos AliExpress em tempo real
            </p>
          </div>

          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="flex items-center gap-2">
                  <Filter className="h-5 w-5" />
                  Filtros de Busca
                </CardTitle>
                <Button variant="outline" size="sm" onClick={clearFilters}>
                  <X className="h-4 w-4 mr-1" />
                  Limpar
                </Button>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-4 md:grid-cols-2">
                <div>
                  <Label htmlFor="keyword">Palavra-chave</Label>
                  <Input
                    id="keyword"
                    placeholder="Ex: smartwatch, headphones"
                    value={filters.keyword}
                    onChange={(e) =>
                      setFilters({ ...filters, keyword: e.target.value })
                    }
                    onKeyDown={(e) =>
                      e.key === "Enter" && !isLoading && searchProducts()
                    }
                  />
                  <p className="text-xs text-muted-foreground mt-1">
                    Deixe vazio para produtos em alta
                  </p>
                </div>
                <div>
                  <Label>Categorias</Label>
                  <MultiSelect
                    options={categories}
                    selected={selectedCategories}
                    onChange={setSelectedCategories}
                    placeholder={
                      loadingCategories
                        ? "Carregando categorias..."
                        : "Selecione categorias..."
                    }
                    emptyMessage="Nenhuma categoria encontrada"
                  />
                  <p className="text-xs text-muted-foreground mt-1">
                    Selecione uma ou mais categorias para filtrar
                  </p>
                </div>
              </div>

              <div className="grid gap-4 md:grid-cols-3">
                <div>
                  <Label htmlFor="minPrice">Preço Mínimo (R$)</Label>
                  <Input
                    id="minPrice"
                    type="number"
                    step="0.01"
                    placeholder="10.00"
                    value={filters.minSalePrice}
                    onChange={(e) =>
                      setFilters({ ...filters, minSalePrice: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="maxPrice">Preço Máximo (R$)</Label>
                  <Input
                    id="maxPrice"
                    type="number"
                    step="0.01"
                    placeholder="200.00"
                    value={filters.maxSalePrice}
                    onChange={(e) =>
                      setFilters({ ...filters, maxSalePrice: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="sort">Ordenação</Label>
                  <Select
                    value={filters.sort}
                    onValueChange={(value) =>
                      setFilters({ ...filters, sort: value })
                    }
                  >
                    <SelectTrigger id="sort">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">Padrão</SelectItem>
                      <SelectItem value="SALE_PRICE_ASC">
                        Menor Preço
                      </SelectItem>
                      <SelectItem value="SALE_PRICE_DESC">
                        Maior Preço
                      </SelectItem>
                      <SelectItem value="LAST_VOLUME_ASC">
                        Menos Vendidos
                      </SelectItem>
                      <SelectItem value="LAST_VOLUME_DESC">
                        Mais Vendidos
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="grid gap-4 md:grid-cols-3">
                <div>
                  <Label htmlFor="minRating">Rating Mínimo</Label>
                  <Input
                    id="minRating"
                    type="number"
                    step="0.1"
                    placeholder="4.5"
                    value={filters.minRating}
                    onChange={(e) =>
                      setFilters({ ...filters, minRating: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="minOrders">Pedidos Mínimos</Label>
                  <Input
                    id="minOrders"
                    type="number"
                    placeholder="100"
                    value={filters.minOrders}
                    onChange={(e) =>
                      setFilters({ ...filters, minOrders: e.target.value })
                    }
                  />
                </div>
                <div>
                  <Label htmlFor="pageSize">Itens por Página</Label>
                  <Select
                    value={pageSize.toString()}
                    onValueChange={(value) => {
                      setPageSize(parseInt(value));
                      setCurrentPage(1);
                    }}
                  >
                    <SelectTrigger id="pageSize">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="10">10</SelectItem>
                      <SelectItem value="20">20</SelectItem>
                      <SelectItem value="30">30</SelectItem>
                      <SelectItem value="50">50</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="flex gap-2 pt-2">
                <Button
                  onClick={searchProducts}
                  disabled={isLoading}
                  className="flex-1 gap-2"
                >
                  {isLoading ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Buscando produtos...
                    </>
                  ) : (
                    <>
                      <Search className="h-4 w-4" />
                      Buscar Produtos
                    </>
                  )}
                </Button>
              </div>
            </CardContent>
          </Card>

          {products.length > 0 && (
            <Card>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle>
                    {products.length} produto(s) encontrado(s)
                  </CardTitle>
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-muted-foreground">
                      Página {currentPage}
                    </span>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                  {products.map((product) => (
                    <Card
                      key={product.id || product.externalId}
                      className="overflow-hidden hover:shadow-lg transition-shadow cursor-pointer"
                      onClick={() => handleProductClick(product)}
                    >
                      <div className="aspect-square relative bg-muted">
                        <Image
                          src={product.imageUrl || "/placeholder.svg"}
                          alt={product.name}
                          fill
                          className="object-cover"
                        />
                        <div className="absolute top-2 right-2">
                          <ScoreBadge score={product.score} />
                        </div>
                      </div>
                      <CardContent className="p-4">
                        <h3 className="font-semibold text-sm line-clamp-2 mb-2 min-h-[40px]">
                          {product.name}
                        </h3>
                        <div className="space-y-2">
                          <div className="flex items-center justify-between">
                            <span className="text-xs text-muted-foreground">
                              Fornecedor
                            </span>
                            <span className="text-sm font-medium">
                              R$ {product.supplierPrice.toFixed(2)}
                            </span>
                          </div>
                          <div className="flex items-center justify-between">
                            <span className="text-xs text-muted-foreground">
                              Venda
                            </span>
                            <span className="text-sm font-bold text-green-600">
                              R$ {product.estimatedSalePrice.toFixed(2)}
                            </span>
                          </div>
                          <div className="flex items-center justify-between">
                            <span className="text-xs text-muted-foreground">
                              Margem
                            </span>
                            <Badge variant="secondary">
                              {product.marginPercent.toFixed(1)}%
                            </Badge>
                          </div>
                          <div className="flex items-center gap-1 text-xs text-muted-foreground">
                            <Star className="h-3 w-3 fill-yellow-400 text-yellow-400" />
                            <span>{product.rating}</span>
                            <span></span>
                            <ShoppingCart className="h-3 w-3" />
                            <span>{product.orders} pedidos</span>
                          </div>
                          <div className="flex gap-1">
                            <CompetitionBadge
                              level={
                                product.competitionLevel.toLowerCase() as any
                              }
                            />
                            <SentimentBadge
                              sentiment={product.sentiment.toLowerCase() as any}
                            />
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}

          {products.length > 0 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                Mostrando {products.length} produtos da página {currentPage}
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handlePageChange(1)}
                  disabled={currentPage === 1 || isLoading}
                >
                  <ChevronsLeft className="h-4 w-4" />
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage === 1 || isLoading}
                >
                  <ChevronLeft className="h-4 w-4" />
                  Anterior
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={isLoading}
                >
                  Próxima
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
            </div>
          )}

          {!isLoading && products.length === 0 && (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <Search className="h-12 w-12 text-muted-foreground mb-4" />
                <h3 className="text-lg font-semibold mb-2">
                  Nenhum produto encontrado
                </h3>
                <p className="text-sm text-muted-foreground text-center max-w-md">
                  Use os filtros acima para buscar produtos do AliExpress em
                  tempo real. Deixe a palavra-chave vazia para produtos em alta.
                </p>
              </CardContent>
            </Card>
          )}
        </div>
      </main>

      <ProductModal
        product={selectedProduct}
        open={modalOpen}
        onClose={() => setModalOpen(false)}
      />
    </div>
  );
}
