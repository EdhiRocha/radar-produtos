import { cn } from "@/lib/utils";
import { ThumbsUp, ThumbsDown, Minus } from 'lucide-react';

interface SentimentBadgeProps {
  sentiment: "positivo" | "negativo" | "misto";
}

export function SentimentBadge({ sentiment }: SentimentBadgeProps) {
  const config = {
    positivo: {
      icon: ThumbsUp,
      className: "bg-green-100 text-green-800 border-green-200",
      label: "Positivo",
    },
    negativo: {
      icon: ThumbsDown,
      className: "bg-red-100 text-red-800 border-red-200",
      label: "Negativo",
    },
    misto: {
      icon: Minus,
      className: "bg-yellow-100 text-yellow-800 border-yellow-200",
      label: "Misto",
    },
  };

  const { icon: Icon, className, label } = config[sentiment];

  return (
    <span
      className={cn(
        "inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-semibold",
        className
      )}
    >
      <Icon className="h-3 w-3" />
      {label}
    </span>
  );
}
