import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { getApi, postApi, putApi, deleteApi } from "../../../api";
import { IComment } from "../../../interfaces/IComment";
import { formStyle } from "../../../styles/formStyle";

export function CommentsSection() {
    const [comments, setComments] = useState<IComment[]>([]);
    const [edit, setEdit] = useState<IComment | undefined>();
    const [selected, setSelected] = useState<Set<number>>(new Set());

    const defaults = useMemo<IComment>(() => ({
        id: undefined,
        email: "",
        text: "",
        isApproved: false
    }), []);

    const { register, handleSubmit, reset } = useForm<IComment>({
        defaultValues: defaults
    });

    const load = () => getApi<IComment[]>("comments").then(c => c && setComments(c));

    useEffect(() => { load(); }, []);
    useEffect(() => { reset(edit ?? defaults); }, [edit, reset, defaults]);

    const onSubmit = handleSubmit((data, e) => {
        const nativeEvent = e?.nativeEvent as SubmitEvent | undefined;
        const submitter = nativeEvent?.submitter as HTMLButtonElement | undefined;
        const action = submitter?.value;

        if (action === "delete") {
            if (data.id) deleteApi(`comments/${data.id}`, {}).then(load);
            reset(defaults); setEdit(undefined);
            return;
        }

        if (action === "new") data.id = undefined;

        const task = data.id
            ? putApi(`comments/${data.id}`, data)
            : postApi("comments", data);

        task?.then(() => { load(); if (action === "new") reset(defaults); });
    });

    const toggleSelect = (id?: number) => {
        if (!id) return;
        setSelected(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id); else next.add(id);
            return next;
        });
    };

    const bulkDelete = async () => {
        const ids = Array.from(selected);
        if (ids.length === 0) return;
        await Promise.all(ids.map(id => deleteApi(`comments/${id}`, {})));
        setSelected(new Set());
        await load();
    };

    return (
        <div className="mt-6 mb-20">
            <div className="text-xl mb-2">Komentarai</div>

            <div className="flex items-start gap-4">
              
                <div className="flex-1 min-w-0 max-w-[calc(100%-14rem)]">
                    <table className="w-full border border-[#065f46] border-separate border-spacing-0 mb-3 text-sm">
                        <thead>
                            <tr className="bg-gray-100">
                                <th className="border border-[#065f46] px-2 py-1 text-left font-medium">Vartotojo paštas</th>
                                <th className="border border-[#065f46] px-2 py-1 text-left font-medium">Komentaras</th>
                                <th className="border border-[#065f46] px-2 py-1"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {comments.map(c => (
                                <tr key={c.id} className="border border-[#065f46]">
                                    <td className="border border-[#065f46] px-2 py-1">{c.email}</td>
                                    <td className="border border-[#065f46] px-2 py-1">{c.text}</td>
                                    <td className="border border-[#065f46] px-2 py-1">
                                        <button className="underline text-blue-600 text-xs" onClick={() => setEdit(c)}>Redaguoti</button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

               
                <div className="w-56 shrink-0">
                    <div className="text-sm font-medium mb-2">Pažymėti trinimui</div>
                    <div className="flex flex-col gap-2 mb-3">
                        {comments.map(c => (
                            <label key={c.id} className="flex items-center gap-2 text-sm">
                                <input
                                    type="checkbox"
                                    checked={c.id ? selected.has(c.id) : false}
                                    onChange={() => toggleSelect(c.id)}
                                />
                                <span className="truncate" title={c.email}>{c.email}</span>
                            </label>
                        ))}
                    </div>
                    <button
                        type="button"
                        className={`${formStyle.button} w-full`}
                        onClick={bulkDelete}
                        disabled={selected.size === 0}
                        title={selected.size === 0 ? "Nieko nepažymėta" : "Ištrinti pažymėtus komentarus"}
                    >
                        Trinti pažymėtus
                    </button>
                </div>
            </div>

          
            <form onSubmit={onSubmit} className="flex flex-col gap-3 mt-4">
                <input type="hidden" {...register("id")} />

                <div className="max-w-md">
                    <label htmlFor="email" className={formStyle.label}>Vartotojo paštas</label>
                    <input
                        id="email"
                        className={`${formStyle.input} max-w-md`}
                        {...register("email", { required: true, maxLength: 100 })}
                    />
                </div>

                <div className="max-w-xl">
                    <label htmlFor="text" className={formStyle.label}>Rašyti komentarą</label>
                    <textarea
                        id="text"
                        rows={3}
                        className={`${formStyle.input} max-w-xl`}
                        {...register("text", { required: true, maxLength: 1000 })}
                    />
                </div>

                <div className="flex flex-col sm:flex-row gap-2">
                    <button className={formStyle.button} type="submit" value="update">Išsaugoti</button>
                    <button className={formStyle.button} type="submit" value="new">Naujas</button>
                    <button className={formStyle.button} type="submit" value="delete">Ištrinti</button>
                </div>
            </form>
        </div>
    );
}

export default CommentsSection;