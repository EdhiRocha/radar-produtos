import { cn } from "@/lib/utils";

interface CompetitionBadgeProps {
  level: "baixa" | "media" | "alta";
}

export function CompetitionBadge({ level }: CompetitionBadgeProps) {
  const config = {
    baixa: {
      className: "bg-green-100 text-green-800 border-green-200",
      label: "Baixa",
    },
    media: {
      className: "bg-yellow-100 text-yellow-800 border-yellow-200",
      label: "Média",
    },
    alta: {
      className: "bg-red-100 text-red-800 border-red-200",
      label: "Alta",
    },
  };

  const { className, label } = config[level];

  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold",
        className
      )}
    >
      {label}
    </span>
  );
}
