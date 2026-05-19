import { useEffect, useMemo } from "react";
import { useForm } from "react-hook-form";
import { ISoil } from "../../../interfaces/ISoil";
import { formStyle } from "../../../styles/formStyle";

interface Props {
    storeSoil: (s: ISoil) => void;
    soil?: ISoil;
    deleteSoil: (id?: number) => void;
}

export function SoilForm({ storeSoil, soil, deleteSoil }: Props) {
    const defaults = useMemo<ISoil>(
        () => ({
            id: undefined,
            name: "",
        }),
        []
    );

    const { register, handleSubmit, reset } = useForm<ISoil>({
        defaultValues: defaults,
    });

    useEffect(() => {
        reset(soil ?? defaults);
    }, [soil, reset, defaults]);

    const onSubmit = handleSubmit((data, e) => {
        const nativeEvent = e?.nativeEvent as SubmitEvent | undefined;
        const submitter = nativeEvent?.submitter as HTMLButtonElement | undefined;
        const action = submitter?.value;

        if (action === "delete") {
            if (data.id) {
                deleteSoil?.(data.id);
            }
            reset(defaults);
            return;
        }

        if (action === "new") {
            data.id = undefined;
        }

        storeSoil(data);

        if (action === "new") {
            reset(defaults);
        }
    });

    return (
        <form onSubmit={onSubmit} className="flex flex-col gap-3">
            <input type="hidden" {...register("id")} />

            <div>
                <label htmlFor="name" className={formStyle.label}>Pavadinimas</label>
                <input
                    id="name"
                    className={formStyle.input}
                    {...register("name", { required: true, maxLength: 100 })}
                />
            </div>

            <div className="flex flex-col sm:flex-row gap-2">
                <button className={formStyle.button} type="submit" value="update">Atnaujinti</button>
                <button className={formStyle.button} type="submit" value="new">Naujas</button>
                <button className={formStyle.button} type="submit" value="delete">Ištrinti</button>
            </div>
        </form>
    );
}

export default SoilForm;