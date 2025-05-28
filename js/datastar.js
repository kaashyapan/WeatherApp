import {
	apply,
	load,
	setAlias,
} from "../node_modules/@starfederation/datastar/dist/engine";
import { DELETE } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/actions/delete";
import { GET } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/actions/get";
import { PATCH } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/actions/patch";
import { POST } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/actions/post";
import { PUT } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/actions/put";
import { Indicator } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/attributes/indicator";
import { ExecuteScript } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/watchers/executeScript";
import { MergeFragments } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/watchers/mergeFragments";
import { MergeSignals } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/watchers/mergeSignals";
import { RemoveFragments } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/watchers/removeFragments";
import { RemoveSignals } from "../node_modules/@starfederation/datastar/dist/plugins/official/backend/watchers/removeSignals";
import { Clipboard } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/actions/clipboard";
import { CustomValidity } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/customValidity";
import { OnIntersect } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/onIntersect";
import { OnInterval } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/onInterval";
import { OnLoad } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/onLoad";
import { OnRaf } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/onRaf";
import { OnSignalChange } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/onSignalChange";
import { Persist } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/persist";
import { ReplaceUrl } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/replaceUrl";
import { ScrollIntoView } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/scrollIntoView";
import { ViewTransition } from "../node_modules/@starfederation/datastar/dist/plugins/official/browser/attributes/viewTransition";
import { Attr } from "../node_modules/@starfederation/datastar/dist/plugins/official/dom/attributes/attr";
import { Bind } from "../node_modules/@starfederation/datastar/dist/plugins/official/dom/attributes/bind";
import { Class } from "../node_modules/@starfederation/datastar/dist/plugins/official/dom/attributes/class";
import { On } from "../node_modules/@starfederation/datastar/dist/plugins/official/dom/attributes/on";
import { Ref } from "../node_modules/@starfederation/datastar/dist/plugins/official/dom/attributes/ref";
import { Show } from "../node_modules/@starfederation/datastar/dist/plugins/official/dom/attributes/show";
import { Text } from "../node_modules/@starfederation/datastar/dist/plugins/official/dom/attributes/text";
import { Fit } from "../node_modules/@starfederation/datastar/dist/plugins/official/logic/actions/fit";
import { SetAll } from "../node_modules/@starfederation/datastar/dist/plugins/official/logic/actions/setAll";
import { ToggleAll } from "../node_modules/@starfederation/datastar/dist/plugins/official/logic/actions/toggleAll";

load(
	// DOM
	Attr,
	Bind,
	Class,
	On,
	Ref,
	Show,
	Text,
	// Backend
	Indicator,
	GET,
	POST,
	PUT,
	PATCH,
	DELETE,
	MergeFragments,
	MergeSignals,
	RemoveFragments,
	RemoveSignals,
	ExecuteScript,
	// Browser
	Clipboard,
	CustomValidity,
	OnIntersect,
	OnInterval,
	OnLoad,
	OnRaf,
	OnSignalChange,
	Persist,
	ReplaceUrl,
	ScrollIntoView,
	ViewTransition,
	// Logic
	Fit,
	SetAll,
	ToggleAll,
);

apply();

export { apply, load, setAlias };
