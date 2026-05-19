import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { IPlant } from "../../../interfaces/IPlant";
import { IPest } from "../../../interfaces/IPest";
import { ISoil } from "../../../interfaces/ISoil";
import { getApi } from "../../../api";
import { formStyle } from "../../../styles/formStyle";

interface Props {
    storePlant: (p: IPlant) => void;
    plant?: IPlant;
    deletePlant: (id?: number) => void;
}

export function PlantForm({ storePlant, plant, deletePlant }: Props) {
    const defaults = useMemo<IPlant>(
        () => ({
            id: undefined,
            name: "",
            description: "",
            soilType: "",
            pests: "",
            pestControlMethod: "",
            imageUrl: "",
        }),
        []
    );

    const { register, handleSubmit, reset, setValue } = useForm<IPlant>({
        defaultValues: defaults,
    });

    // Pests dropdown state (kept)
    const [allPests, setAllPests] = useState<IPest[]>([]);
    const [selectedPestIds, setSelectedPestIds] = useState<number[]>([]);

    // Soils dropdown state (now matches pests)
    const [soils, setSoils] = useState<ISoil[]>([]);
    const [selectedSoilIds, setSelectedSoilIds] = useState<number[]>([]);

    // Load pests
    useEffect(() => {
        getApi<IPest[]>("pests").then((p) => { if (p) setAllPests(p); });
    }, []);

    // Load soils
    useEffect(() => {
        getApi<ISoil[]>("soils").then((s) => { if (s) setSoils(s); });
    }, []);

    // Initialize from plant for pests and soil
    useEffect(() => {
        reset(plant ?? defaults);

        // Keep the name contract for soil and pests strings
        setValue("soilType", plant?.soilType ?? "");
        setValue("pests", plant?.pests ?? "");

        // Map existing comma-separated pest names to IDs when editing
        if (plant?.pests && allPests.length > 0) {
            const names = plant.pests
                .split(",")
                .map((s) => s.trim())
                .filter((s) => s.length > 0);
            const ids = names
                .map((n) => allPests.find((p) => p.name.toLowerCase() === n.toLowerCase())?.id)
                .filter((id): id is number => typeof id === "number");
            setSelectedPestIds(ids);
            const normalized = ids
                .map((id) => allPests.find((p) => p.id === id)?.name)
                .filter((x): x is string => !!x)
                .join(", ");
            setValue("pests", normalized);
        } else if (!plant?.pests) {
            setSelectedPestIds([]);
            setValue("pests", "");
        }

        // Map existing comma-separated soil names to IDs when editing
        if (plant?.soilType && soils.length > 0) {
            const names = plant.soilType
                .split(",")
                .map((s) => s.trim())
                .filter((s) => s.length > 0);
            const ids = names
                .map((n) => soils.find((s) => s.name.toLowerCase() === n.toLowerCase())?.id)
                .filter((id): id is number => typeof id === "number");
            setSelectedSoilIds(ids);
            const normalized = ids
                .map((id) => soils.find((s) => s.id === id)?.name)
                .filter((x): x is string => !!x)
                .join(", ");
            setValue("soilType", normalized);
        } else if (!plant?.soilType) {
            setSelectedSoilIds([]);
            setValue("soilType", "");
        }
    }, [plant, reset, defaults, allPests, soils, setValue]);

    // Keep hidden "pests" field in sync with selections
    const updateSelected = (ids: number[]) => {
        setSelectedPestIds(ids);
        const names = ids
            .map((id) => allPests.find((p) => p.id === id)?.name)
            .filter((x): x is string => !!x);
        setValue("pests", names.join(", "));
    };

    const onSelectChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const selected = Array.from(e.target.selectedOptions).map((o) => Number(o.value));
        updateSelected(selected);
    };

    const removeSelected = (id: number) => {
        updateSelected(selectedPestIds.filter((x) => x !== id));
    };

    // Keep hidden "soilType" field in sync with selections (same pattern as pests)
    const updateSelectedSoils = (ids: number[]) => {
        setSelectedSoilIds(ids);
        const names = ids
            .map((id) => soils.find((s) => s.id === id)?.name)
            .filter((x): x is string => !!x);
        setValue("soilType", names.join(", "));
    };

    const onSoilSelectChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const selected = Array.from(e.target.selectedOptions).map((o) => Number(o.value));
        updateSelectedSoils(selected);
    };

    const removeSelectedSoil = (id: number) => {
        updateSelectedSoils(selectedSoilIds.filter((x) => x !== id));
    };

    const onSubmit = handleSubmit((data, e) => {
        const nativeEvent = e?.nativeEvent as SubmitEvent | undefined;
        const submitter = nativeEvent?.submitter as HTMLButtonElement | undefined;
        const action = submitter?.value;

        if (action === "delete") {
            if (data.id) deletePlant?.(data.id);
            reset(defaults);
            updateSelected([]);       // reset pests
            updateSelectedSoils([]);  // reset soils
            return;
        }

        if (action === "new") {
            data.id = undefined;
        }

        // soilType and pests already synchronized
        storePlant(data);

        if (action === "new") {
            reset(defaults);
            updateSelected([]);       // reset pests
            updateSelectedSoils([]);  // reset soils
        }
    });

    return (
        <form onSubmit={onSubmit} className="flex flex-col gap-3">
            <input type="hidden" {...register("id")} />
            {/* hidden fields carrying the comma-separated names */}
            <input type="hidden" {...register("pests")} />
            <input type="hidden" {...register("soilType")} />

            <div>
                <label htmlFor="name" className={formStyle.label}>Pavadinimas</label>
                <input
                    id="name"
                    className={formStyle.input}
                    {...register("name", { required: true, maxLength: 100 })}
                />
            </div>

            <div>
                <label htmlFor="description" className={formStyle.label}>Aprašymas</label>
                <textarea
                    id="description"
                    className={formStyle.input}
                    rows={3}
                    {...register("description", { maxLength: 500 })}
                />
            </div>

            {/* Soils multi-select (now same UX as pests) */}
            <div>
                <label htmlFor="soilSelect" className={formStyle.label}>Dirvožemiai</label>
                <select
                    id="soilSelect"
                    multiple
                    className={formStyle.input}
                    value={selectedSoilIds.map(String)}
                    onChange={onSoilSelectChange}
                >
                    {soils.map((s) => (
                        <option key={s.id} value={s.id}>
                            {s.name}
                        </option>
                    ))}
                </select>

                {selectedSoilIds.length > 0 && (
                    <div className="mt-2 flex flex-wrap gap-2">
                        {selectedSoilIds.map((id) => {
                            const name = soils.find((s) => s.id === id)?.name ?? id;
                            return (
                                <span
                                    key={id}
                                    className="inline-flex items-center gap-2 bg-gray-200 rounded px-2 py-1 text-sm"
                                >
                                    {name}
                                    <button
                                        type="button"
                                        className="text-red-600"
                                        onClick={() => removeSelectedSoil(id)}
                                        aria-label={`Pašalinti ${name}`}
                                    >
                                        &times;
                                    </button>
                                </span>
                            );
                        })}
                    </div>
                )}
            </div>

            {/* Pests multi-select (kept) */}
            <div>
                <label htmlFor="pestSelect" className={formStyle.label}>Kenkėjai</label>
                <select
                    id="pestSelect"
                    multiple
                    className={formStyle.input}
                    value={selectedPestIds.map(String)}
                    onChange={onSelectChange}
                >
                    {allPests.map((p) => (
                        <option key={p.id} value={p.id}>
                            {p.name}
                        </option>
                    ))}
                </select>

                {selectedPestIds.length > 0 && (
                    <div className="mt-2 flex flex-wrap gap-2">
                        {selectedPestIds.map((id) => {
                            const name = allPests.find((p) => p.id === id)?.name ?? id;
                            return (
                                <span key={id} className="inline-flex items-center gap-2 bg-gray-200 rounded px-2 py-1 text-sm">
                                    {name}
                                    <button
                                        type="button"
                                        className="text-red-600"
                                        onClick={() => removeSelected(id)}
                                        aria-label={`Pašalinti ${name}`}
                                    >
                                        &times;
                                    </button>
                                </span>
                            );
                        })}
                    </div>
                )}
            </div>

            <div>
                <label htmlFor="pestControlMethod" className={formStyle.label}>Kenkėjų kontrolės metodas</label>
                <input
                    id="pestControlMethod"
                    className={formStyle.input}
                    {...register("pestControlMethod", { maxLength: 200 })}
                />
            </div>

            <div>
                <label htmlFor="imageUrl" className={formStyle.label}>Nuotraukos URL</label>
                <input
                    id="imageUrl"
                    type="url"
                    className={formStyle.input}
                    {...register("imageUrl", { maxLength: 300 })}
                />
            </div>

            <div className="flex flex-col sm:flex-row gap-2">
                <button className={formStyle.button} type="submit" value="update">Atnaujinti</button>
                <button className={formStyle.button} type="submit" value="new">Pridėti naują</button>
                <button className={formStyle.button} type="submit" value="delete">Ištrinti</button>
            </div>
        </form>
    );
}

export default PlantForm;